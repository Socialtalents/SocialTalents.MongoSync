@Echo off
powershell -Command "Invoke-WebRequest https://github.com/Socialtalents/SocialTalents.MongoSync/raw/master/SocialTalents.MongoSync.Console/Publish/Release.zip -OutFile MongoSync.zip"
Echo Installing MongoSync to MongoSync.suo so it will be invisble for git
powershell -Command "Expand-Archive MongoSync.zip -DestinationPath MongoSync.suo -Force"
copy MongoSync.suo\MongoSync.bat .
del MongoSync.zip
                                                                                                                              
