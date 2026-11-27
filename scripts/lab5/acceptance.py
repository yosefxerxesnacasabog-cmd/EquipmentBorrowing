"""Exercise the supplied Lab 5 cases against a migrated disposable PostgreSQL DB."""
import argparse
import datetime as dt
import json
import os
from pathlib import Path
import subprocess
import time
import urllib.error
import urllib.request

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'docs/evidence/lab5'
BASE = 'http://127.0.0.1:5185'
DB = 'equipment_borrowing_lab5_test'
PSQL = os.environ.get('PSQL', 'psql')
TODAY = dt.date.today()
DUE = str(TODAY + dt.timedelta(days=2))
server = None
log = None
records = []
spec = json.loads((ROOT / 'src/EquipmentBorrowing.Api/openapi.json').read_text())


def sql(query=None, file=None):
    args = [PSQL, '-X', '-q', '-t', '-A', '-v', 'ON_ERROR_STOP=1', '-d', DB]
    args += ['-f', str(file)] if file else ['-c', query]
    return subprocess.run(args, check=True, capture_output=True, text=True).stdout.strip()


def snapshot():
    return json.loads(sql('''SELECT json_build_object(
      'students', (SELECT coalesce(json_agg(t ORDER BY t."Id"),'[]') FROM "Students" t),
      'equipment', (SELECT coalesce(json_agg(t ORDER BY t."Id"),'[]') FROM "Equipment" t),
      'borrowings', (SELECT coalesce(json_agg(t ORDER BY t."Id"),'[]') FROM "Borrowings" t));'''))


def validate(value, schema):
    if '$ref' in schema:
        schema = spec['components']['schemas'][schema['$ref'].split('/')[-1]]
    if value is None and schema.get('nullable'):
        return
    kind = schema.get('type')
    if kind == 'object':
        assert isinstance(value, dict), (value, schema)
        assert set(schema.get('required', [])) <= value.keys(), (value, schema)
        for key, val in value.items():
            if key in schema.get('properties', {}):
                validate(val, schema['properties'][key])
    elif kind == 'array':
        assert isinstance(value, list)
        for val in value:
            validate(val, schema['items'])
    elif kind == 'integer':
        assert type(value) is int
    elif kind == 'boolean':
        assert type(value) is bool
    elif kind == 'string':
        assert isinstance(value, str), (value, schema)
        if schema.get('format') == 'date':
            assert str(dt.date.fromisoformat(value)) == value
    if 'enum' in schema:
        assert value in schema['enum'], (value, schema)


def req(method, route, body=None, expected=200, raw=False):
    mutation = method != 'GET'
    before = snapshot() if mutation else None
    data = body.encode() if raw else json.dumps(body).encode() if body is not None else None
    request = urllib.request.Request(BASE + route, data=data, method=method,
                                     headers={'Content-Type': 'application/json'})
    try:
        response = urllib.request.urlopen(request, timeout=12)
    except urllib.error.HTTPError as error:
        response = error
    content = response.read().decode()
    value = json.loads(content) if content else None
    entry = {'method': method, 'route': route, 'request': body, 'expectedStatus': expected,
             'observedStatus': response.code, 'location': response.headers.get('Location'),
             'response': value}
    if mutation:
        entry['before'] = before
        entry['after'] = snapshot()
    records.append(entry)
    assert response.code == expected, entry
    path = route.split('?')[0]
    operation = None
    for template, methods in spec['paths'].items():
        segments, target = template.split('/'), path.split('/')
        if len(segments) == len(target) and all(a == b or a.startswith('{') for a, b in zip(segments, target)):
            operation = methods.get(method.lower())
            break
    if operation:
        contract = operation['responses'][str(expected)]
        schema = contract.get('content', {}).get('application/json', {}).get('schema')
        if schema:
            validate(value, schema)
    if expected >= 400:
        assert set(value) == {'code', 'message'}
        assert not any(s in content.lower() for s in ('password=', 'stacktrace', 'npgsql.', ' at microsoft.'))
        if mutation:
            assert before == entry['after'], entry
    print(f'PASS {method} {route} -> {response.code}', flush=True)
    return value, entry


def get(route):
    return req('GET', '/api/v1/' + route)[0]


def write(method, route, body, expected):
    return req(method, '/api/v1/' + route, body, expected)


def start(broken=False):
    global server, log
    stop()
    env = os.environ.copy()
    if broken:
        # Only this API process uses the invalid address; PostgreSQL is untouched.
        env['EQUIPMENT_BORROWING_CONNECTION'] = 'Host=127.0.0.1;Port=1;Database=unreachable;Username=test;Timeout=1'
    log = open(OUT / ('server-failure.log' if broken else 'server.log'), 'a', encoding='utf-8')
    server = subprocess.Popen(['dotnet', 'bin/Debug/net10.0/EquipmentBorrowing.Api.dll', '--urls', BASE],
                              cwd=ROOT / 'src/EquipmentBorrowing.Api', env=env, stdout=log, stderr=log)
    for _ in range(80):
        if server.poll() is not None:
            raise RuntimeError('API exited; inspect local server log')
        try:
            urllib.request.urlopen(BASE + '/openapi/v1.json', timeout=1).close()
            return
        except (urllib.error.URLError, TimeoutError):
            time.sleep(.15)
    raise RuntimeError('API did not start')


def stop():
    global server, log
    if server is not None:
        server.terminate()
        server.wait(timeout=10)
        server = None
    if log is not None:
        log.close()
        log = None


def reset():
    sql(file=ROOT / 'scripts/lab5/reset.sql')
    students = {x['studentNumber']: x['id'] for x in get('students')}
    equipment = {x['equipmentCode']: x['id'] for x in get('equipment')}
    records.append({'reset': True, 'freshPostgreSQL': snapshot()})
    return students, equipment


def checkout(s, e, expected=201, batch=False, due=DUE):
    return write('POST', 'checkout-requests' if batch else 'borrowings',
                 {'studentId': s, 'equipmentIds' if batch else 'equipmentId': e,
                  'expectedReturnDate': due}, expected)


def t01():
    s, e = reset()
    before = snapshot()
    assert len(get('equipment')) == 7 and len(get('students')) == 4
    available = get('equipment?available=true')
    assert {x['equipmentCode'] for x in available} == {'EQ-002', 'EQ-003', 'EQ-006', 'EQ-007'}
    assert get(f'students/{s["ST-100"]}')['fullName'] == 'Ada Cruz'
    assert get(f'equipment/{e["EQ-001"]}')['isAvailable'] is False
    loans = get(f'students/{s["ST-100"]}/borrowings')
    assert len(loans) == 2 and {x['status'] for x in loans} == {'Active', 'Returned'}
    assert len(get(f'equipment/{e["EQ-001"]}/borrowings')) == 1
    assert len(get('borrowings?status=Active')) == 3
    assert get(f'students/{s["ST-400"]}/borrowings') == []
    get(f'borrowings/{loans[0]["id"]}')
    for route in ['students/999999', 'equipment/999999', 'borrowings/999999', 'missing-route']:
        req('GET', '/api/v1/' + route, expected=404)
    req('DELETE', '/api/v1/equipment', expected=405)
    served, _ = req('GET', '/openapi/v1.json')
    assert served == spec
    operations = [v for methods in spec['paths'].values() for k, v in methods.items() if k in ('get', 'post', 'put', 'patch')]
    assert {x['operationId'] for x in operations} == {f'API-{i:02}' for i in range(1, 17)}
    assert before == snapshot()
    records.append({'openApiOperations': len(operations), 'unchangedPostgreSQL': snapshot()})


def t02():
    s, e = reset()
    student, created = write('POST', 'students', {'studentNumber': 'ST-500', 'fullName': 'Eli Santos', 'contact': None}, 201)
    assert student['isEligible'] is True
    assert get(created['location'].removeprefix('/api/v1/')) == student
    edited, _ = write('PUT', f'students/{student["id"]}', {'studentNumber': 'ST-500', 'fullName': 'Eli Santos Updated', 'contact': 'Local test contact'}, 200)
    write('POST', 'students', {'studentNumber': 'ST-501', 'fullName': '   '}, 400)
    write('POST', 'students', {'studentNumber': ' st-100 ', 'fullName': 'Duplicate'}, 409)
    ada = s['ST-100']
    old = get(f'students/{ada}/borrowings')
    write('PATCH', f'students/{ada}/eligibility', {'isEligible': False}, 200)
    assert len(get(f'students/{ada}/borrowings')) == len(old)
    checkout(ada, e['EQ-002'], 409)
    active = next(x for x in old if x['status'] == 'Active')
    write('POST', f'borrowings/{active["id"]}/return', {}, 200)
    write('PATCH', f'students/{ada}/eligibility', {'isEligible': True}, 200)
    checkout(ada, e['EQ-002'])
    write('PATCH', f'students/{ada}/eligibility', {'isEligible': True}, 200)
    before = snapshot()
    start()
    assert get(f'students/{student["id"]}') == edited
    assert len(get(f'students/{ada}/borrowings')) == 3
    assert snapshot() == before
    records.append({'restartReadback': snapshot()})


def t03():
    s, e = reset()
    item, created = write('POST', 'equipment', {'equipmentCode': 'EQ-008', 'name': 'Scanner', 'description': None}, 201)
    assert item['isAvailable'] is True and get(created['location'].removeprefix('/api/v1/')) == item
    write('PUT', f'equipment/{item["id"]}', {'equipmentCode': 'EQ-008', 'name': 'Scanner Updated', 'description': 'USB'}, 200)
    history = get(f'equipment/{e["EQ-001"]}/borrowings')
    camera, _ = write('PUT', f'equipment/{e["EQ-001"]}', {'equipmentCode': 'EQ-001', 'name': 'Camera Updated', 'description': None}, 200)
    assert camera['isAvailable'] is False
    assert [x['id'] for x in get(f'equipment/{e["EQ-001"]}/borrowings')] == [x['id'] for x in history]
    write('POST', 'equipment', {'equipmentCode': 'EQ-009', 'name': ' '}, 400)
    write('POST', 'equipment', {'equipmentCode': ' eq-001 ', 'name': 'Duplicate'}, 409)
    write('PUT', f'equipment/{e["EQ-001"]}', {'equipmentCode': 'EQ-001', 'name': 'Camera', 'description': None, 'isAvailable': True}, 400)
    before = snapshot()
    start()
    assert get(f'equipment/{e["EQ-001"]}') == camera
    assert get(f'equipment/{item["id"]}')['name'] == 'Scanner Updated'
    assert before == snapshot()
    records.append({'restartReadback': snapshot()})


def t04():
    s, e = reset()
    loan, created = checkout(s['ST-400'], e['EQ-002'])
    assert get(created['location'].removeprefix('/api/v1/')) == loan
    assert loan['borrowedDate'] == str(TODAY) and loan['status'] == 'Active'
    assert not get(f'equipment/{e["EQ-002"]}')['isAvailable']
    checkout(s['ST-200'], e['EQ-006'], 409)
    checkout(s['ST-400'], e['EQ-001'], 409)
    checkout(999999, e['EQ-006'], 404)
    checkout(s['ST-400'], 999999, 404)
    checkout(s['ST-400'], e['EQ-006'], 400, due=str(TODAY))
    checkout(s['ST-400'], e['EQ-006'], 400, due='not-a-date')
    returned, _ = write('POST', f'borrowings/{loan["id"]}/return', {}, 200)
    assert returned['returnedDate'] == str(TODAY) and returned['status'] == 'Returned'
    write('POST', f'borrowings/{loan["id"]}/return', {}, 409)
    before = snapshot()
    start()
    assert get(f'borrowings/{loan["id"]}') == returned
    assert get(f'equipment/{e["EQ-002"]}')['isAvailable']
    assert before == snapshot()
    records.append({'restartReadback': snapshot()})


def t05():
    s, e = reset()
    result, _ = checkout(s['ST-400'], [e['EQ-002'], e['EQ-006']], batch=True)
    assert len(result['borrowings']) == 2
    assert len(snapshot()['borrowings']) == 6
    s, e = reset()
    checkout(s['ST-300'], e['EQ-002'])
    assert len([x for x in get(f'students/{s["ST-300"]}/borrowings') if x['status'] == 'Active']) == 3
    s, e = reset()
    checkout(s['ST-100'], [e['EQ-002'], e['EQ-006']], batch=True)
    assert len([x for x in get(f'students/{s["ST-100"]}/borrowings') if x['status'] == 'Active']) == 3
    cases = [('ST-300', ['EQ-002', 'EQ-006'], 409, DUE),
             ('ST-400', ['EQ-002', 'EQ-002'], 400, DUE),
             ('ST-400', ['EQ-002'], 400, DUE),
             ('ST-400', ['EQ-001', 'EQ-002'], 409, DUE),
             ('ST-200', ['EQ-002', 'EQ-006'], 409, DUE),
             ('ST-400', ['EQ-002', 'MISSING'], 404, DUE),
             ('ST-400', ['EQ-002', 'EQ-006'], 400, str(TODAY))]
    for student, items, status, due in cases:
        s, e = reset()
        checkout(s[student], [e.get(x, 999999) for x in items], status, batch=True, due=due)
    s, e = reset()
    sql('''CREATE SEQUENCE lab5_fault_probe;
      CREATE FUNCTION lab5_fault() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN
        IF NEW."StudentId" = (SELECT "Id" FROM "Students" WHERE "StudentNumber"='ST-400') THEN
          IF nextval('lab5_fault_probe') = 2 THEN
            IF NOT EXISTS(SELECT 1 FROM "Borrowings" WHERE "StudentId"=NEW."StudentId") THEN
              RAISE EXCEPTION 'First insert was not visible';
            END IF;
            PERFORM nextval('lab5_fault_probe');
            RAISE EXCEPTION 'Controlled fault after first insert';
          END IF;
        END IF;
        RETURN NEW;
      END $$;
      CREATE TRIGGER lab5_fault BEFORE INSERT ON "Borrowings" FOR EACH ROW EXECUTE FUNCTION lab5_fault();''')
    try:
        checkout(s['ST-400'], [e['EQ-002'], e['EQ-006']], 500, batch=True)
        assert sql('SELECT last_value FROM lab5_fault_probe') == '3'
        records.append({'faultProbe': 3, 'failurePoint': 'Second INSERT; third sequence increment proves first INSERT visible inside transaction; all application changes rolled back', 'freshPostgreSQL': snapshot()})
    finally:
        sql('DROP TRIGGER lab5_fault ON "Borrowings"; DROP FUNCTION lab5_fault(); DROP SEQUENCE lab5_fault_probe;')


def t06():
    s, e = reset()
    before = snapshot()
    assert len(get(f'students/{s["ST-100"]}/borrowings')) == 2
    assert get(f'students/{s["ST-400"]}/borrowings') == []
    for code in ['EQ-001', 'EQ-003']:
        assert len(get(f'equipment/{e[code]}/borrowings')) == 1
    for route in ['students/999999/borrowings', 'equipment/999999/borrowings']:
        req('GET', '/api/v1/' + route, expected=404)
    for query, count in [('aDa', 2), ('st-1', 2), ('aMeR', 1)]:
        assert len(get('borrowings?search=' + query)) == count
    for status, count in [('All', 4), ('Active', 3), ('Returned', 1)]:
        assert len(get('borrowings?status=' + status)) == count
    for field in ['borrowedDate', 'expectedReturnDate']:
        for direction in ['asc', 'desc']:
            rows = get(f'borrowings?sort={field}&direction={direction}')
            expected = sorted(rows, key=lambda x: x['id'])
            expected.sort(key=lambda x: x[field], reverse=direction == 'desc')
            assert rows == expected
    combined = get('borrowings?search=aDa&status=Returned&sort=expectedReturnDate&direction=desc')
    assert len(combined) == 1 and combined[0]['equipment']['equipmentCode'] == 'EQ-003'
    assert len(get('borrowings')) == 4
    assert get('borrowings?search=no-such-person') == []
    req('GET', '/api/v1/borrowings?status=Invalid', expected=400)
    assert before == snapshot()
    records.append({'unchangedPostgreSQL': snapshot()})


def t08():
    s, e = reset()
    loan, _ = checkout(s['ST-400'], e['EQ-002'])
    before = snapshot()
    stop()
    try:
        urllib.request.urlopen(BASE + '/api/v1/equipment', timeout=2)
        raise AssertionError('Expected no HTTP response')
    except urllib.error.URLError:
        records.append({'transportFailure': 'API stopped: connection refused, no HTTP response or status'})
    start(broken=True)
    req('GET', '/api/v1/equipment', expected=500)
    write('POST', 'students', {'studentNumber': 'FAIL-001', 'fullName': 'Must Not Persist'}, 500)
    assert before == snapshot()
    start()
    assert get(f'borrowings/{loan["id"]}') == loan
    assert len(get('students')) == 4
    assert snapshot() == before
    records.append({'recoveredAfterRestart': snapshot()})


def main():
    global records
    parser = argparse.ArgumentParser()
    parser.add_argument('--serve', action='store_true')
    parser.add_argument('--only', choices=['T01', 'T02', 'T03', 'T04', 'T05', 'T06', 'T08'])
    args = parser.parse_args()
    assert os.environ.get('PGDATABASE') == DB, 'Set PGDATABASE to the disposable test database.'
    assert f'Database={DB}'.lower() in os.environ.get('EQUIPMENT_BORROWING_CONNECTION', '').lower(), 'API must use the same test database.'
    assert sql('SELECT current_database()') == DB
    OUT.mkdir(parents=True, exist_ok=True)
    start()
    try:
        if args.serve:
            reset()
            print('READY for desktop walkthrough', flush=True)
            while True:
                command = OUT / '.serve-command'
                if command.exists():
                    mode = command.read_text().strip()
                    command.unlink()
                    if mode == 'stop':
                        stop()
                    elif mode == 'failure':
                        start(broken=True)
                    elif mode == 'recover':
                        start()
                    elif mode == 'finish':
                        break
                    print('CONTROL ' + mode, flush=True)
                time.sleep(1)
            return
        for name, test in [('T01', t01), ('T02', t02), ('T03', t03), ('T04', t04), ('T05', t05), ('T06', t06), ('T08', t08)]:
            if args.only and args.only != name:
                continue
            records = []
            result = {'test': name, 'localDate': str(TODAY), 'status': 'FAILED', 'captures': records}
            try:
                test()
                result['status'] = 'PASSED'
            except Exception as error:
                result['failure'] = str(error)
                raise
            finally:
                (OUT / f'{name}.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
            print(name + ' PASSED', flush=True)
    finally:
        stop()


if __name__ == '__main__':
    main()
