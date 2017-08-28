# NR-CORE

NR-CORE is a Realm of the Mad God private server. It represents the continuation of the test source used to run Nilly's Realm and is an attempt to bring the broader rotmg private server community together so that we may take a little more enjoyment out of a game that we've all come to love.

Note that this source is very much a WIP. Nothing is guaranteed to work.

## Getting Started
The server consists of two primary components, the app engine (server) and the world server (wServer). The app engine is used to handle http get and post requests while the world server is used to run the game world. The source, as a whole, uses [redis](https://redis.io/) for data persistence. The RemoteLogger is optionally run on the server to provide a means of saving logging information.

To interact with the server you'll need a client capable of doing so. A client is currently being worked on for this source. You may find it [here](https://github.com/cp-nilly/NR-27.7.X13). 

As a push to get prospective private server owners to add some variety to their server, assets (resources) have been removed from this repository. You'll need to provide the server and wServer with these resources before they will run. A basic compilation of these resources can be found [here](https://nillysrealm.com/topic/19811/nr-core-resource-pack).

Once you you have redis configured and running, you're able to build the source, and you've downloaded a basic set of resources, you'll need to make sure the server.json and wServer.json is configured properly. Most importantly is that you configure where your resources are located (resourceFolder). After that initial configuration, you can start having fun customizing your server as you see fit.

## Behaviors
Behaviors have been removed from the source as they are closely tied to the server assets. To add behaviors to your source, you either need to create the necessary files or get them from somewhere and place them in **wServer/logic/db**. You'll have to create the db folder as it likely will not exist initially. Once placed in the db folder, close the project in your ide and open it. At that point, all the added behaviors should present themselves.

Some behaviors have been included in the resource compilation linked in the getting started section. They should give you an idea of how the behavior system works and what you'll need to do to add new ones.

## Configuring the Initial Admin
Currently, the first admin on the server will need to be manually configured via the database. After that, given that the first admin is of rank 100, that admin can rank other players. The [dbschema.txt](https://github.com/cp-nilly/NR-CORE/blob/master/common/dbSchema.txt) outlines the structure of the database data. The keys that need to be changed is the admin and rank fields of the account you want to give admin to.

An example of manually ranking a player via redis-cli (the default client supplied with redis):
```
127.0.0.1:6379> hget names NILLY
"5"
127.0.0.1:6379> hset account.5 admin 1
(integer) 0
127.0.0.1:6379> hset account.5 rank 100
(integer) 0
127.0.0.1:6379>
```

## Getting Additional Help
If you need additional help regarding this source, think about joining the NR community.

* Forum: https://nillysrealm.com/category/20/nr-core-testing
* Discord: https://discord.gg/hmMTQWk

## Pioneering Credits
The following list of individuals each played a special role in making this source what it is today. Aside from creepylava, they've all worked on the nr test source at one point or another.

- [creepylava](https://github.com/creepylava) - Created the original source this work is based off of. Without him, this would not exist.
- [cp-nilly](https://github.com/cp-nilly)
- [tuvior](https://github.com/tuvior)
- [ossimc82](https://github.com/ossimc82)
- [TheSnowQueen](https://github.com/TheSnowQueen)
- [Cyeclops](https://github.com/Cyeclops)
- [Varanus-Komodoensis](https://github.com/Varanus-Komodoensis)
- [Moloch-horridus](https://github.com/Moloch-horridus)

## Thanks to the NR community
A special thanks goes out to the NR community for making the Nilly's Realm test server as successful as it has been. They were, and always have been, the driving force that allowed this project to survive as long as it has.
