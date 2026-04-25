# OpenLDAP C# Playground

โปรเจกต์นี้เป็นตัวตั้งต้นสำหรับลองต่อ C# Minimal API เข้ากับ OpenLDAP แบบ local

## สิ่งที่มีให้แล้ว

- `docker-compose.yml` สำหรับรัน OpenLDAP และ phpLDAPadmin
- LDIF bootstrap สำหรับสร้างข้อมูลตัวอย่างอัตโนมัติ
- Minimal API สำหรับทดสอบ bind และ search
- Config LDAP ผ่าน `appsettings.json`
- Swagger UI สำหรับยิง API ง่าย ๆ

## โครงสร้าง

```text
.
├── docker-compose.yml
├── openldap-cs.sln
└── src
    └── OpenLdap.Api
```

## เริ่มต้นใช้งาน

### 1. ติดตั้ง .NET SDK

แนะนำ .NET 8 SDK

### 2. รัน OpenLDAP

```bash
docker compose up -d
```

เปิด phpLDAPadmin ได้ที่ [http://localhost:8081](http://localhost:8081)

- Login DN: `cn=admin,dc=example,dc=org`
- Password: `admin`

มี sample user ให้แล้ว:

- User DN: `uid=john,ou=people,dc=example,dc=org`
- Password: `john123`

### 3. restore และรัน API

```bash
dotnet restore
dotnet run --project src/OpenLdap.Api
```

Swagger จะขึ้นที่ URL ที่ console ของ `dotnet run` แจ้งไว้ เช่น `http://localhost:5047/swagger`

## API ที่เตรียมไว้

### POST `/ldap/test-bind`

ตัวอย่าง request

```json
{
  "distinguishedName": "uid=john,ou=people,dc=example,dc=org",
  "password": "john123"
}
```

### GET `/ldap/users`

ค้นหา user แบบง่ายจาก `inetOrgPerson`

```text
GET /ldap/users
GET /ldap/users?uid=john
```

### POST `/ldap/search`

ค้นหาแบบกำหนด filter เอง

```json
{
  "baseDn": "dc=example,dc=org",
  "filter": "(objectClass=*)",
  "attributes": ["cn", "uid", "mail"],
  "sizeLimit": 50
}
```

## หมายเหตุ

- ตอนนี้ repo นี้ถูก scaffold ให้พร้อม แต่เครื่องนี้ยังไม่มี `dotnet` ติดตั้ง จึงยังไม่ได้รัน build/test จริง
- ถ้าจะเพิ่ม seed user สำหรับ LDAP ผมช่วยต่อให้ได้อีก เช่น LDIF sample และ endpoint create user
