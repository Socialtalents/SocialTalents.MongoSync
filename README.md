# SocialTalents.MongoSync
MongoDb data sync tool, inspired by Liquibase.
Supports inserts, updates, drop, delete, create index, and eval of any script.
Works for Mongo 3.4+ or later.

# Installation in dev environment (windows)

1. Create a folder wihtin a project, e.g. MongoSync and place install_windows.bat in it.
2. Run install_windows.bat
3. Fresh release will be downloaded and extracted to MongoSync.suo folder (.suo so git will ignore it)
4. Add install_windows.bat to git so others can easily install it (and install_linux.sh if you deploy to linux)

# Collect some changes

Execute following command to export whole collection:
```
MongoSync export --conn mongodb://localhost/database --collection Config 
```

Execute following command to export some query collection:
```
MongoSync export --conn mongodb://localhost/database --collection Config --query {myProperty:2}
```

It is getting more tricky when you want to use quotes. For windows, use single quotation. For linux, you have to escape them with \:
```
#windows
MongoSync export --conn mongodb://localhost/database --collection Config --query {myProperty:'argument'}
#linux
MongoSync export --conn mongodb://localhost/database --collection Config --query {myProperty:\'argument\'}
```


MongoSync will generate file which you need to include within your deployment, e.g:
`636517244.Config.Insert.json`

3rd component define operation. You can rename file to use different insert mode (see mongoimport documentation):
```
636517244.Config.Upsert.json
636517244.Config.Merge.json
```

## Delete objects
Add file with Delete operation, put a search query into body:
`636517244.Config.Delete.json`
File content:
```
{}
```

## Drop colleciton
```
636517244.Config.Drop.json
(File content ignored)
````

## CreateIndex
Add file with an index definition in body:
```
636517244.Config.CreateIndex.js
{ Type: 1, IndexKey: 1 }, { unique: true, partialFilterExpression: { IndexKey: { $exists: true } } }
```

## Eval any javascript
```
636517244.Config.eval.js
printjson(db.getCollectionNames());
```

# Deployment

Here is script for linux, but for windows logic is the same. 

Navigate to folder with scripts and json files, install mongosync tool, run import.

In case you need to install dotnet core follow first 2 steps from official documentation: https://www.microsoft.com/net/learn/get-started/linuxubuntu 

```
cd /MongoSync
sh install_linux.sh
sh mongosync.sh import --conn mongodb://user:password@127.0.0.1:29017/database
```

For vsts, we use "Run shell commands or a script on a remote machine using SSH" step as Agent phase so it executed only once.

# Issues

Known issue: windows script contains hardcoded path to mongo 3.6 folder, you need to fix it manually or add mongo to path
