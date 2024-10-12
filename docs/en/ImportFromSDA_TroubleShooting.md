# Problems with importing maFile from SDA 

If maFile is not imported and the application returns the error "Import error: 1", then you need to make sure that maFile is not encrypted: 
Open the file in any text editor and check that it has JSON format and starts with: 
``` {"shared_secret": "........ ``` 
## In case of failure: 
- Open the old SDA application - Click "Configure encryption" 
- ![screenshot](https://private-user-images.githubusercontent.com/106531132/375905410-72c2f355-b88b-4b96-8dc8-540be302a11b.png?jwt=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3Mjg3MDUwODUsIm5iZiI6MTcyODcwNDc4NSwicGF0aCI6Ii8xMDY1MzExMzIvMzc1OTA1NDEwLTcyYzJmMzU1LWI4OGItNGI5Ni04ZGM4LTU0MGJlMzAyYTExYi5wbmc_WC1BbXotQWxnb3JpdGhtPUFXUzQtSE1BQy1TSEEyNTYmWC1BbXotQ3JlZGVudGlhbD1BS0lBVkNPRFlMU0E1M1BRSzRaQSUyRjIwMjQxMDEyJTJGdXMtZWFzdC0xJTJGczMlMkZhd3M0X3JlcXVlc3QmWC1BbXotRGF0ZT0yMDI0MTAxMlQwMzQ2MjVaJlgtQW16LUV4cGlyZXM9MzAwJlgtQW16LVNpZ25hdHVyZT04ZWI3MDY4MzU1ZjgyZTExMmM4NjA4YmRiMTUxMzk3YWI1NDE4M2I1NzU0YTc1ZmM2OTg1ZTkxZjZhODNhZGM5JlgtQW16LVNpZ25lZEhlYWRlcnM9aG9zdCJ9.ThNMfA24bVN-kCmxbjGNfQz4QX9mXs8FzfeTEm9pctw) 
- Enter old password if required 
- Leave new password blank and continue 
# Your files are now decrypted and you can import them into Nebula