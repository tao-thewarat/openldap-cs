# OpenLDAP C# Playground

โปรเจกต์นี้ใช้สำหรับลอง ASP.NET Core Web API คู่กับ OpenLDAP แบบ local ผ่าน Docker

## สิ่งที่มีให้แล้ว

- `docker-compose.yml` สำหรับรัน OpenLDAP และ phpLDAPadmin
- LDIF sample สำหรับ import user ตัวอย่าง
- ค่าตั้งต้น LDAP ใน `appsettings.json`
- ASP.NET Core Web API พร้อม Swagger
- pre-commit สำหรับ format, build และ test

## เริ่ม OpenLDAP

รันจาก root ของโปรเจกต์:

```bash
docker compose up -d
```

เช็กสถานะ:

```bash
docker compose ps
```

ถ้าจะดู log:

```bash
docker compose logs -f openldap
docker compose logs -f phpldapadmin
```

## ค่าเข้าใช้งาน

OpenLDAP:

- Host: `localhost`
- Port: `389`
- Base DN: `dc=example,dc=org`
- Admin DN: `cn=admin,dc=example,dc=org`
- Admin Password: `admin`

phpLDAPadmin:

- URL: [http://localhost:8081](http://localhost:8081)
- Login DN: `cn=admin,dc=example,dc=org`
- Password: `admin`

ไฟล์ตัวอย่างสำหรับ import:

- [docker/ldif/01-bootstrap.ldif](/Users/taotoxicboy/Documents/projects/openldap-cs/docker/ldif/01-bootstrap.ldif)

## Bind/Search ตัวอย่าง

ถ้ามี `ldapsearch` ในเครื่อง:

```bash
ldapsearch -x -H ldap://localhost -D "cn=admin,dc=example,dc=org" -w admin -b "dc=example,dc=org"
```

ถ้าจะ import sample data:

```bash
docker exec -i openldap-local ldapadd -x -D "cn=admin,dc=example,dc=org" -w admin < docker/ldif/01-bootstrap.ldif
```

หลัง import แล้วลอง bind ด้วย sample user:

```bash
ldapwhoami -x -H ldap://localhost -D "uid=john,ou=people,dc=example,dc=org" -w john123
```

## รัน API

```bash
dotnet restore
dotnet watch run
```

Swagger ปกติจะอยู่ที่ [http://localhost:5025/swagger](http://localhost:5025/swagger)

## Reset ข้อมูล LDAP

ถ้าจะล้าง volume แล้วเริ่มใหม่:

```bash
docker compose down -v
docker compose up -d
```

## โครงสร้างที่เกี่ยวข้อง

```text
.
├── docker-compose.yml
├── docker/
│   └── ldif/
│       └── 01-bootstrap.ldif
├── Controllers/
├── DTOs/
├── Program.cs
└── appsettings.json
```
