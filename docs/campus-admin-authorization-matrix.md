# Campus Admin Authorization Matrix

## Role intent
- `SUPER_ADMIN`: system-wide governance and unrestricted management.
- `CENTER_ADMIN`: campus-scoped operations/admin management.

## Permission matrix (high-level)

| Capability | SUPER_ADMIN | CENTER_ADMIN |
|---|---|---|
| Global role/permission governance | ✅ | ❌ |
| Global users CRUD | ✅ | ❌ |
| Campus users CRUD (own campus) | ✅ | ✅ |
| Teachers/classes/rooms/schedules in own campus | ✅ | ✅ |
| Cross-campus data access | ✅ | ❌ |

## Whitelist role assignment for CENTER_ADMIN

`CENTER_ADMIN` may only assign/remove/replace these role codes:
- `STAFF`
- `TEACHER`
- `PARENT`
- `STUDENT`

All role assignment changes by `CENTER_ADMIN` are additionally restricted to users in the same campus.
