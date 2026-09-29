# Steam Release Analytics API

[![CI](https://github.com/SourceSurfer/SteamReleaseAnalytics/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/SourceSurfer/SteamReleaseAnalytics/actions/workflows/ci.yml?query=branch%3Amain) [![Tests](https://img.shields.io/endpoint?url=https%3A%2F%2Fraw.githubusercontent.com%2FSourceSurfer%2FSteamReleaseAnalytics%2Fbadges%2Ftests.json)](https://github.com/SourceSurfer/SteamReleaseAnalytics/actions/workflows/ci.yml?query=branch%3Amain)

Backend-сервис на ASP.NET Core для хранения и анализа данных о релизах игр в Steam: календарь релизов по месяцам, статистика и динамика популярности жанров. Данные об играх вносятся через REST API; автоматический сбор из Steam — в планах (см. «Будущие улучшения»).

## 🎯 Основные возможности

- **Календарь релизов** - получение игр по месяцам с группировкой по дням
- **Аналитика жанров** - топ-5 популярных жанров с статистикой
- **Анализ динамики** - отслеживание изменений популярности жанров за 3 месяца
- **JWT аутентификация** - POST и DELETE требуют токен с ролью Admin (демо-пользователь, см. «Аутентификация»)
- **REST API** - стандартный API для интеграции с фронтенд-приложениями
- **Swagger документация** - интерактивная документация API

## 🛠️ Технологический стек

- **Backend**: C# и ASP.NET Core 10 (.NET 10 LTS)
- **ORM**: Entity Framework Core 10 + Npgsql, миграции в репозитории
- **Database**: PostgreSQL 16
- **Аутентификация**: JWT токены
- **Контейнеризация**: Docker и Docker Compose
- **API документация**: Swagger / OpenAPI (Swashbuckle)
- **Тесты**: xUnit, NSubstitute, FluentAssertions, EF Core InMemory, WebApplicationFactory
- **Архитектура**: монолит с разделением на проекты Api / Services / Infrastructure / Core

## 📋 Структура проекта

```
SteamReleaseAnalytics/
├── SteamReleaseAnalytics.Api/           # REST API и контроллеры
│   ├── Controllers/
│   │   ├── GamesController.cs           # Эндпоинты для работы с играми
│   │   ├── AnalyticsController.cs       # Эндпоинты аналитики
│   │   └── AuthController.cs            # Вход демо-пользователя, выдача JWT
│   ├── Auth/                            # Демо-учётка (DemoUserOptions) и роли
│   ├── Swagger/                         # Схема Bearer для защищённых операций
│   ├── Validation/                      # Допустимый период запросов (год, формат месяца)
│   ├── Program.cs                       # Конфигурация приложения
│   ├── SteamReleaseAnalytics.Api.http   # Готовые запросы к API (VS / Rider / VS Code REST Client)
│   └── appsettings.json                 # Настройки
├── SteamReleaseAnalytics.Core/          # Модели и DTO
│   ├── Models/                          # Entity модели
│   │   ├── Game.cs
│   │   ├── Tag.cs
│   │   ├── GameTag.cs
│   │   └── GameSnapshot.cs
│   └── Dtos/                            # Data Transfer Objects
│       ├── GameDto.cs
│       ├── CreateGameDto.cs
│       ├── GameCalendarDto.cs
│       ├── GenreStatsDto.cs
│       └── GenreDynamicsDto.cs
├── SteamReleaseAnalytics.Infrastructure/# Работа с БД и репозитории
│   ├── Data/
│   │   └── SteamDbContext.cs            # Entity Framework контекст
│   ├── Migrations/                      # Миграции БД (EF Core): InitialCreate, MakeOptionalGameFieldsNullable
│   └── Repositories/                    # Репозитории
│       ├── IGameRepository.cs
│       ├── GameRepository.cs
│       ├── ITagRepository.cs
│       ├── TagRepository.cs
│       ├── IGameSnapshotRepository.cs
│       └── GameSnapshotRepository.cs
├── SteamReleaseAnalytics.Services/      # Бизнес-сервисы
│   ├── Services/
│   │   ├── IAnalyticsService.cs
│   │   └── AnalyticsService.cs
│   └── Security/
│       ├── IJwtTokenGenerator.cs
│       ├── JwtOptions.cs                # Настройки JWT, проверяются при старте
│       └── JwtTokenGenerator.cs
├── tests/SteamReleaseAnalytics.Tests/   # Тесты (xUnit, NSubstitute, FluentAssertions)
│   ├── Api/                             # Контроллеры, валидация DTO, интеграционные тесты конвейера
│   ├── Services/                        # AnalyticsService, JwtTokenGenerator
│   └── Infrastructure/                  # Репозитории и модель EF на InMemory
├── .github/workflows/ci.yml             # CI: сборка, тесты, значок числа тестов
├── dotnet-tools.json                    # Локальный инструмент dotnet-ef
├── docker-compose.yml                   # Docker Compose конфигурация
├── Dockerfile                           # Docker образ для API
├── LICENSE                              # Лицензия MIT
└── README.md                            # Документация
```

## 🚀 Быстрый старт

### Требования

- Docker Desktop (или Docker + Docker Compose)
- .NET 10 SDK (для локальной разработки)

### Запуск через Docker Compose

1. **Перейдите в папку проекта**:
```bash
cd SteamReleaseAnalytics
```

2. **Запустите контейнеры**:
```bash
docker compose up --build
```

Это автоматически:
- Запустит PostgreSQL базу данных
- Запустит pgAdmin для управления БД
- Соберёт и запустит API приложение
- Применит миграции и создаст все необходимые таблицы

3. **Откройте приложение**:
- **API и Swagger**: http://localhost:8080/swagger/index.html
- **pgAdmin**: http://localhost:5050 (admin@example.com / admin)
- **PostgreSQL**: localhost:5432 (postgres / postgres)

### Запуск локально (API без Docker)

1. **Поднимите PostgreSQL** — из Docker Compose или свой PostgreSQL 16 с настройками из `appsettings.json`:
```bash
docker compose up -d postgres
```

2. **Запустите API**:
```bash
dotnet run --project SteamReleaseAnalytics.Api
```

Миграции применяются при старте автоматически (переменная `Database__ApplyMigrationsOnStartup` задана в `Properties/launchSettings.json`).

API будет доступна на: http://localhost:5268/swagger/index.html
(HTTPS: `dotnet run --project SteamReleaseAnalytics.Api --launch-profile https` → https://localhost:7205/swagger/index.html)

Применить миграции вручную, без запуска API:
```bash
dotnet tool restore
dotnet ef database update --project SteamReleaseAnalytics.Infrastructure --startup-project SteamReleaseAnalytics.Api
```

## 📚 API Эндпоинты

### Аутентификация

**POST** `/api/v1/auth/login`
- Получить JWT токен демо-пользователя (роль Admin)
- Body: `{"username": "admin", "password": "admin"}`
- Response: `{"token": "eyJhbGc..."}`; неверные логин или пароль — 401, пустые — 400

### Игры

**GET** `/api/v1/games`
- Получить все игры
- Response: массив GameDto

**GET** `/api/v1/games/{id}`
- Получить игру по ID
- Response: GameDto

**POST** `/api/v1/games` (роль Admin)
- Создать новую игру
- Body: CreateGameDto — обязательны только `steamAppId` и `title`
- Response: GameDto (код 201); игра с таким ID уже есть или тело не прошло валидацию — 400

**DELETE** `/api/v1/games/{id}` (роль Admin)
- Удалить игру
- Response: код 204; неизвестный ID — 404

**GET** `/api/v1/games/calendar?month=2025-11`
- Получить календарь релизов на месяц (строго `YYYY-MM`, год 1970–2100)
- Response: GameCalendarDto

**GET** `/api/v1/games/by-tag/{tagName}`
- Получить игры по тегу/жанру
- Response: массив GameDto

### Аналитика

**GET** `/api/v1/analytics/top-genres?month=11&year=2025`
- Получить топ-5 жанров за месяц (месяц 1–12, год 1970–2100)
- Response: массив GenreStatsDto

**GET** `/api/v1/analytics/genre-dynamics`
- Получить динамику топ-5 жанров за последние 3 месяца (текущий и два предыдущих); топ-5 выбирается по числу игр за все три месяца
- Response: массив GenreDynamicsDto

### Валидация и ошибки

`CreateGameDto` проверяется до вызова контроллера:

| Поле | Правило |
|---|---|
| `steamAppId` | ≥ 1 |
| `title` | обязательно, до 500 символов |
| `description` | до 2000 символов |
| `imageUrl`, `storeUrl` | URL (http, https или ftp), до 500 символов |
| `followers` | ≥ 0 |
| `platforms` | до 200 символов |
| `tags` | до 20 тегов, каждый непустой и до 100 символов; дубликаты схлопываются |
| `releaseDate` | дата без часового пояса считается UTC |

Ошибки валидации тела запроса, 401/403/404 без тела и непредвиденные ошибки возвращаются в формате [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) (`application/problem+json`): 400 — с перечнем полей, 500 — без деталей исключения и stack trace. Проверки параметров в контроллерах (месяц, год, повторный `steamAppId`) отвечают 400 с текстом ошибки.

## 🔐 Аутентификация

API использует JWT токены для защиты изменяющих операций: POST и DELETE требуют токен с ролью `Admin` (без токена — 401, с другой ролью — 403).

1. Получите токен через `/api/v1/auth/login` с логином и паролем демо-пользователя
2. Добавьте токен в заголовок Authorization:
```
Authorization: Bearer <ваш_токен>
```

В Swagger UI токен вводится через кнопку **Authorize**. Токен действителен 60 минут (настраивается в `appsettings.json`).

> **Демо-пользователь.** Учётная запись одна и задаётся конфигурацией `Auth:DemoUser`. В `launchSettings.json` и `docker-compose.yml` это `admin` / `admin`; в `appsettings.json` она пустая, и без неё вход отключён. Для реального использования нужно хранилище пользователей с хэшированными паролями.

## 🗄️ База данных

Схема создаётся миграциями EF Core из `SteamReleaseAnalytics.Infrastructure/Migrations`.

### Таблицы

- **Games** - основная таблица с информацией об играх
- **Tags** - теги/жанры игр
- **GameTags** - связь между играми и тегами (M:N)
- **GameSnapshots** - исторические данные о числе подписчиков (зарезервировано под сбор данных из Steam, API пока не использует)

### Схема базы данных

```sql
-- Games таблица содержит:
- SteamAppId (Primary Key)
- Title
- Description
- ReleaseDate
- ImageUrl
- StoreUrl
- Followers
- Platforms (Windows, Mac, Linux)
- CreatedAt
- UpdatedAt

-- Tags таблица содержит:
- Id (Primary Key)
- Name (Unique)

-- GameTags таблица содержит:
- Id (Primary Key)
- GameSteamAppId (FK to Games)
- TagId (FK to Tags)

-- GameSnapshots таблица содержит:
- Id (Primary Key)
- GameSteamAppId (FK to Games)
- FollowersCount
- SnapshotDate
```

## ✅ Как запустить тесты

```bash
dotnet test
```

Тесты не требуют ни PostgreSQL, ни сети: репозитории подменяются через NSubstitute, а сами репозитории проверяются на EF Core InMemory. Покрыты расчёт статистики жанров (`AnalyticsService`), выдача и проверка JWT (`JwtTokenGenerator`), выборка релизов по месяцу и тегу, календарь релизов и валидация запросов в контроллерах.

Интеграционные тесты поднимают API целиком через `WebApplicationFactory` (с подменёнными репозиториями) и проверяют то, что живёт в конвейере ASP.NET Core: валидацию модели и ProblemDetails, вход и роли, отказ стартовать без секрета JWT, схему Bearer в Swagger и CORS.

## 🧪 Тестирование API

### Через Swagger UI

1. Откройте http://localhost:8080/swagger/index.html
2. Выполните `POST /api/v1/auth/login` с `{"username": "admin", "password": "admin"}` и скопируйте `token`
3. Нажмите **Authorize** и вставьте токен — после этого доступны POST и DELETE (они помечены замком)

### Через .http файл

`SteamReleaseAnalytics.Api/SteamReleaseAnalytics.Api.http` содержит готовые запросы ко всем эндпоинтам; токен из запроса логина подставляется автоматически. Адрес по умолчанию — локальный запуск (`http://localhost:5268`); для Docker замените `@host` на `http://localhost:8080`.

### Через Postman

1. **Получите токен**:
   - POST `http://localhost:8080/api/v1/auth/login`
   - Body: `{"username": "admin", "password": "admin"}`

2. **Используйте токен**:
   - Добавьте заголовок: `Authorization: Bearer <token>`
   - Выполняйте POST/DELETE запросы

### Примеры запросов

**Создать игру**:
```bash
curl -X POST http://localhost:8080/api/v1/games \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "steamAppId": 1091500,
    "title": "Cyberpunk 2077",
    "description": "An open-world action RPG",
    "releaseDate": "2025-11-15T00:00:00Z",
    "imageUrl": "https://example.com/image.jpg",
    "storeUrl": "https://store.steampowered.com/app/1091500",
    "followers": 150000,
    "platforms": "Windows,Mac",
    "tags": ["Action", "RPG", "Sci-Fi"]
  }'
```

**Получить календарь на ноябрь**:
```bash
curl http://localhost:8080/api/v1/games/calendar?month=2025-11
```

**Получить топ жанров**:
```bash
curl "http://localhost:8080/api/v1/analytics/top-genres?month=11&year=2025"
```

## 📊 Архитектура

### Многоуровневая архитектура

1. **API Layer** (Controllers) - обработка HTTP запросов
2. **Service Layer** - бизнес-логика (Analytics, Security)
3. **Repository Layer** - работа с БД через Entity Framework
4. **Data Access Layer** - Entity Framework DbContext
5. **Database Layer** - PostgreSQL

### Паттерны

- **Repository Pattern** - для работы с БД
- **Dependency Injection** - через встроенный DI контейнер ASP.NET Core
- **DTO Pattern** - для передачи данных между слоями
- **JWT** - для аутентификации

## 🔧 Конфигурация

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=steam_releases;Username=postgres;Password=postgres;"
  },
  "JwtSettings": {
    "Secret": "",
    "ExpirationMinutes": 60,
    "Issuer": "SteamReleaseAnalytics",
    "Audience": "SteamReleaseAnalyticsUsers"
  },
  "Auth": {
    "DemoUser": { "Username": "", "Password": "" }
  },
  "Cors": {
    "AllowedOrigins": []
  }
}
```

Секретов в `appsettings.json` нет: строка подключения — локальное значение по умолчанию для разработки, а ключ подписи JWT и демо-учётка задаются снаружи. Для локального запуска и Docker Compose они уже прописаны в `launchSettings.json` и `docker-compose.yml` (значения только для разработки); в любой другой среде — через переменные окружения, например `JwtSettings__Secret`.

| Параметр | Назначение |
|---|---|
| `ConnectionStrings:DefaultConnection` | Строка подключения к PostgreSQL |
| `JwtSettings:Secret` | Ключ подписи JWT, не короче 32 символов. **Обязателен**: без него приложение не стартует |
| `JwtSettings:*` | Срок жизни, издатель и аудитория токенов |
| `Auth:DemoUser:Username` / `Password` | Демо-учётка для `/api/v1/auth/login`; пустая — вход отключён |
| `Cors:AllowedOrigins` | Список origin, которым разрешены кросс-доменные запросы; пустой — CORS выключен |
| `Database:ApplyMigrationsOnStartup` | `true` — применять миграции при старте (по умолчанию выключено; включено в `launchSettings.json` и `docker-compose.yml`) |

## 🚨 Важные замечания

- **Среда**: Production в Docker (без HTTPS для упрощения)
- **Безопасность**: В Production используйте настоящие сертификаты HTTPS
- **Секрет JWT**: Значение из `launchSettings.json` / `docker-compose.yml` — только для разработки; в других средах задайте свой случайный ключ через `JwtSettings__Secret`
- **БД Backup**: Используйте volume для PostgreSQL для сохранения данных
- **CORS**: Разрешён только для origin из `Cors:AllowedOrigins` (по умолчанию список пуст)
- **Swagger**: Включён во всех средах, включая Production

## 📈 Будущие улучшения

- Интеграция с реальным Steam API для автоматического сбора данных
- ClickHouse для глубокой аналитики
- GraphQL API в дополнение к REST
- Кэширование результатов (Redis)
- Фоновые задачи для синхронизации (Hangfire)
- WebSocket для real-time уведомлений

## 📝 Лицензия

[MIT](LICENSE)

## 👨‍💻 Автор

[SourceSurfer](https://github.com/SourceSurfer)

---

**Для вопросов и проблем**: обратитесь к документации API в Swagger UI
