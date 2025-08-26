# ИСУС - Информационная Система Управления Секретами
## Корпоративный менеджер секретов IT инфраструктуры (Bank Of Secrets) v 0.1

Секрет - это какой-то набор символов, дающий прямой или косвенный доступ к информационной системе или её компонентам. Например: API ключи, пароли, строки подключения, ключи шифрования, токены доступа и т.д.

[Картиночки с результатом работы ниже](#результат)

### Сервис Bank Of Secrets состоит из 4 компонентов:
- #### Backend - код серверной части ИС (ASP.NET Web API): https://github.com/khaydarovR/.net-c-asp-net
- #### Frontend - SPA Клиент на Angular: https://github.com/khaydarovR/SCP.ClientApp
- #### NuGet пакет для удобной работы с API: https://github.com/khaydarovR/BosClient
- #### Расширение для браузера на JS: https://github.com/khaydarovR/SCP.GhromExt

**Проблемы:**
- Хранение важных секретов в коде (в открытом виде)
- Передача важных конфиденциальных данных через мессенджеры
- Расползание секретов (в диалогах мессенджеров)
- Отсутствие контроля доступа людей к разным ИС
- Сложный аудит ИБ

**Решение:**
- Передача и получение секретов в зашифрованном виде (асимметричное шифрование с помощью RSA)
- Хранение секретов в зашифрованном виде на отдельном сервисе (слив БД сервиса или исходного кода ИС не несет угрозы безопасности IT инфраструктуры)
- Удобное потребление/получение секретов с помощью разработанного расширения для браузера/NuGet пакета
- Гибкое управление правами
- Подробное логирование всех действий пользователей, упрощающее дальнейший аудит ИБ IT инфраструктуры

## Диаграмма развертвования:
<img src="https://github.com/khaydarovR/.net-c-asp-net/assets/95288769/1ae3b406-87b5-4b70-9630-5c866989cea6" width="600">


## Схема БД
<img src="https://github.com/khaydarovR/.net-c-asp-net/assets/95288769/7afac31a-8d00-4789-ad68-4df6af49e78c" width="600">


## Результат 
<img src="https://github.com/khaydarovR/.net-c-asp-net/assets/95288769/786c6065-1ea7-49e2-ab64-eb5a4aaa3c9d" width="500">
<img src="https://github.com/khaydarovR/.net-c-asp-net/assets/95288769/07257b15-8434-447e-9ef9-42361c483d8e" width="500">
<img src="https://github.com/khaydarovR/.net-c-asp-net/assets/95288769/e3ad4a94-331b-4c01-8c58-a098dbc180f4" width="500">
<img src="https://github.com/khaydarovR/.net-c-asp-net/assets/95288769/e07af363-3aad-43f6-a545-786199a6e7a1" width="500">
<img src="https://github.com/khaydarovR/.net-c-asp-net/assets/95288769/46f37b92-67ce-48db-8158-1cc789cb56aa" width="500">
<img src="https://github.com/khaydarovR/.net-c-asp-net/assets/95288769/97ee51f8-0f8d-4b77-809f-ee22d8805c87" width="500">






