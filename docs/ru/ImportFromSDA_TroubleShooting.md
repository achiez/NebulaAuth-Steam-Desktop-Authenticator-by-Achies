# Проблемы с импортом maFiles из SDA

Если maFile не импортируется и приложение выдает ошибку "Импорт ошибки: 1", то необходимо убедится что maFile не зашифрован:
Откройте файл в любом текстовом редакторе и проверьте что он имеет формат JSON и начинается с:
```
{"shared_secret": "........
```
## В противном случае:
- Откройте старое приложение SDA
- Нажмите «Настроить шифрование»
- ![screenshot](https://private-user-images.githubusercontent.com/106531132/375905410-72c2f355-b88b-4b96-8dc8-540be302a11b.png?jwt=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3Mjg3MDUwODUsIm5iZiI6MTcyODcwNDc4NSwicGF0aCI6Ii8xMDY1MzExMzIvMzc1OTA1NDEwLTcyYzJmMzU1LWI4OGItNGI5Ni04ZGM4LTU0MGJlMzAyYTExYi5wbmc_WC1BbXotQWxnb3JpdGhtPUFXUzQtSE1BQy1TSEEyNTYmWC1BbXotQ3JlZGVudGlhbD1BS0lBVkNPRFlMU0E1M1BRSzRaQSUyRjIwMjQxMDEyJTJGdXMtZWFzdC0xJTJGczMlMkZhd3M0X3JlcXVlc3QmWC1BbXotRGF0ZT0yMDI0MTAxMlQwMzQ2MjVaJlgtQW16LUV4cGlyZXM9MzAwJlgtQW16LVNpZ25hdHVyZT04ZWI3MDY4MzU1ZjgyZTExMmM4NjA4YmRiMTUxMzk3YWI1NDE4M2I1NzU0YTc1ZmM2OTg1ZTkxZjZhODNhZGM5JlgtQW16LVNpZ25lZEhlYWRlcnM9aG9zdCJ9.ThNMfA24bVN-kCmxbjGNfQz4QX9mXs8FzfeTEm9pctw)
- Введите старый пароль, если требуется
- Оставьте новый пароль пустым и продолжите
# Теперь ваши mafiles расшифрованы, и вы можете импортировать их в Nebula

