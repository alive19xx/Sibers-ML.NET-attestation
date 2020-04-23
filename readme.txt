
1. скачать файл https://aka.ms/mlnet-resources/resnet_v2_101_299.meta
2. скопировать файл resnet_v2_101_299.meta в папку System.IO.Path.GetTempPath() (внутри в папку MLNET, пример: C:\Users\User\AppData\Local\Temp\MLNET\resnet_v2_101_299.meta)
в противном случае трейнер будет пытаться выкачать его с сервера Microsoft
3. Копию resnet_v2_101_299.meta сохранить где-нибудь, потому что Temp папка может быть очищена OC при следующей перезагрузке ПК