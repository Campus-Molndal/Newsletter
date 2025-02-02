# Newsletter
.Net MVC application to manage newsletters

## How to get started

### How to start the application

```bash
dotnet run
```

### How to start the local MongoDB

**Start MongoDB**

```bash
docker compose -f ./infra/mongodb_and_express.yaml up -d
```

**Stop MongoDB**
```bash
docker compose -f ./infra/mongodb_and_express.yaml down
```

**Set appsettings.Development.json**

```json
"DatabaseToUse": "MongoDb"
```

or if you want to try it temporarily, set the environment variable.

**On Linux and Mac**

```bash
export DatabaseToUse=MongoDb
```

**On Windows**

```bash
set DatabaseToUse=MongoDb
```
