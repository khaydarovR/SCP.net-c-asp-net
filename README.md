## Корпортивный менеджер секретов IT инфраструкутры (Bank Of Secrets)
Секрет - это какой то набор символов, дающий прямой или косвенный доступ к информационной системе или его компонентам. Например: api ключи, пароли, строки подключения, ключи шифрования, токны доступа и т.д.

[Картиночки с резултатом работы ниже](##Результат)

### Сервис Bank Of Secrets из 4 компонентов:
- #### Репозиторий серверной части ИС (asp net web api) : https://github.com/khaydarovR/.net-c-asp-net
- #### SPA Клиент на Angular: https://github.com/khaydarovR/SCP.Front
- #### Nuget пакет для убобной работы с API: https://github.com/khaydarovR/BosClient
- #### Расширение для браузера на JS: https://disk.yandex.ru/d/WTaGyoCYPOR5uQ


Проблемы:
- хранение важных секретов в коде (в открытом ввиде)
- передача важных конфиденциальных данных через мессенджеры
- расползание секретов (в диаолгах мессенджеров)
- отсутвие контроля доступа людей к разным ИС
- сложный аудит ИБ

Решение:
- передача и получение секретов в шифрованном виде (ассиметричное шифрование с помощью RSA)
- хранение секретов в шифрованном ввиде на отдельном сервисе (слив БД сервиса или исходного кода ИС не несет угрозы к безопасности IT инфраструктуре)
- удобное потребление\получение секретов с помощью разработанного расширения для браузера\ nuget пакета
- Гибкое управление правами
- Подробное логирование всех действий пользователей упрощающий дальнейший аудит ИБ IT инфрастуктуры

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






