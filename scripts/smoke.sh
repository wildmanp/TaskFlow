#!/usr/bin/env bash

# Smoke test pentru TaskFlow.Api - săptămâna 3
# Rulare: bash scripts/smoke.sh

BASE_URL="${BASE_URL:-http://localhost:5080}"
FAILED=0

check() {
    local expected="$1"
    local method="$2"
    local path="$3"
    local body="${4:-}"

    local args=(-s -o /dev/null -w "%{http_code}" -X "$method" "$BASE_URL$path")

    if [ -n "$body" ]; then
        args+=(-H "Content-Type: application/json" -d "$body")
    fi

    local actual
    actual=$(curl "${args[@]}")

    if [ "$actual" = "$expected" ]; then
        echo "OK   $actual $method $path"
    else
        echo "FAIL $actual (expected $expected) $method $path"
        FAILED=1
    fi
}

create() {
    local path="$1"
    local body="$2"

    curl -s -o /dev/null -D - -X POST "$BASE_URL$path" \
        -H "Content-Type: application/json" \
        -d "$body" |
        tr -d '\r' |
        awk 'tolower($1) == "location:" { print $2 }' |
        sed 's#.*\/##'
}

check_header() {
    local path="$1"
    local header="$2"
    local expected="$3"

    local actual
    actual=$(curl -s -D - -o /dev/null "$BASE_URL$path" \
        -H "$header" |
        tr -d '\r' |
        awk -F ': ' -v name="$expected" \
            'tolower($1) == tolower(name) { print $2 }')

    if [ -n "$actual" ]; then
        echo "OK   header $expected: $actual"
    else
        echo "FAIL missing response header $expected"
        FAILED=1
    fi
}

SUFFIX=$(date +%s)
PROJECT_NAME="Smoke $SUFFIX"

PROJECT_ID=$(create /api/projects "{\"name\":\"$PROJECT_NAME\"}")
SECOND_PROJECT_ID=$(create /api/projects "{\"name\":\"Smoke Second $SUFFIX\"}")

if [ -z "$PROJECT_ID" ] || [ -z "$SECOND_PROJECT_ID" ]; then
    echo "FAIL could not create projects. Is the API running on $BASE_URL?"
    exit 1
fi

TASK_ID=$(create /api/tasks \
    "{\"title\":\"Smoke task $SUFFIX\",\"priority\":\"High\",\"projectId\":$PROJECT_ID}")

if [ -z "$TASK_ID" ]; then
    echo "FAIL could not create task"
    exit 1
fi

echo "Created projects $PROJECT_ID, $SECOND_PROJECT_ID and task $TASK_ID"

# Projects
check 200 GET /api/projects
check 200 GET "/api/projects/$PROJECT_ID"
check 409 POST /api/projects "{\"name\":\"$PROJECT_NAME\"}"
check 400 POST /api/projects '{"name":""}'
check 200 GET "/api/projects/$PROJECT_ID/tasks"
check 404 GET /api/projects/999999/tasks

# Project update and delete
check 204 PUT "/api/projects/$PROJECT_ID" '{"name":"Smoke Updated"}'
check 409 PUT "/api/projects/$SECOND_PROJECT_ID" '{"name":"Smoke Updated"}'
check 409 DELETE "/api/projects/$PROJECT_ID"

# Tasks
check 200 GET /api/tasks
check 200 GET "/api/tasks?status=Todo&priority=High"
check 200 GET "/api/tasks/$TASK_ID"
check 404 GET /api/tasks/999999
check 404 GET /api/tasks/abc
check 400 POST /api/tasks '{"title":""}'
check 400 POST /api/tasks '{"title":"Bad priority","priority":"Urgent"}'
check 400 POST /api/tasks '{"title":"Invalid project","projectId":999999}'
check 204 PUT "/api/tasks/$TASK_ID" '{"title":"Smoke task updated","priority":"Critical"}'
check 204 PATCH "/api/tasks/$TASK_ID/status" '{"status":"InProgress"}'
check 409 PATCH "/api/tasks/$TASK_ID/status" '{"status":"InProgress"}'
check 204 PATCH "/api/tasks/$TASK_ID/status" '{"status":"Done"}'
check 400 PATCH "/api/tasks/$TASK_ID/status" '{}'
check 404 PATCH /api/tasks/999999/status '{"status":"Done"}'

# Tags
TAG_ID=$(create /api/tags "{\"name\":\"SmokeTag$SUFFIX\"}")

if [ -z "$TAG_ID" ]; then
    echo "FAIL could not create tag"
    FAILED=1
else
    echo "Created tag $TAG_ID"
    check 200 GET /api/tags
    check 200 GET "/api/tags/$TAG_ID"
    check 409 POST /api/tags "{\"name\":\"SmokeTag$SUFFIX\"}"
    check 400 POST /api/tags '{"name":""}'
    check 404 GET /api/tags/999999
fi

# Diagnostics and correlation ID
check 200 GET /api/diagnostics/lifetimes
check 400 GET "/api/diagnostics/boom?kind=argument"
check 500 GET /api/diagnostics/boom
check_header /api/tasks "X-Correlation-Id: Smoke-Correlation-123" "X-Correlation-Id"

# OpenAPI
check 200 GET /openapi/v1.json
check 200 GET /scalar/v1

# Cleanup
check 204 DELETE "/api/tasks/$TASK_ID"
check 204 DELETE "/api/projects/$PROJECT_ID"
check 204 DELETE "/api/projects/$SECOND_PROJECT_ID"

if [ "$FAILED" -eq 0 ]; then
    echo ""
    echo "All checks passed"
else
    echo ""
    echo "Some checks failed"
    exit 1
fi