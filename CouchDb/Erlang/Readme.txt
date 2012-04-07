Modifying CouchDb for use with Hydra
------------------------------------

Hydra requires document ids that are monotonically increasing for any CouchDb instance, and globally unique across instances. The files here implement a document id
scheme with these properties, by modifying the couch_uuids.erl file in the CouchDb source at http://git-wip-us.apache.org/repos/asf?p=couchdb.git.

Files
-----

couch_db.hrl		- include file needed for compilation. Taken directly from the CouchDb source.
couch_uuids.erl		- modified version of couch_uuids.erl
couch_uuids.src.erl - original version, taken from the CouchDb source.

When new versions of CouchDb are released, check the source code against couch_uuids.src.erl, and make corresponding modifications to couch_uuids.erl if necessary.
NOTE: do not edit these files in Notepad or Visual Studio. They save files in ways that Erlang does not understand and you get strange syntax and compilation errors. Notepad++ works well though.

Installation
------------

To modify CouchDb, do as follows. Here $CouchDb refers to the CouchDb installation directory, usually C:\Program Files (x86)\Apache Software Foundation\CouchDB

1. Open a command prompt in this directory.
2. Run the Erlang werl.exe program in that command window. This is found in the CouchDb installation tree, at something like $CouchDB\erts-5.8.4\bin\werl.exe.
3. In the resulting window, type
   c(couch_uuids).
   including the final dot, and then hit return. That should yield {ok,couch_uuids}. Type halt().<return> to exit Erlang.
4. You should now have a couch_uuids.beam file in this directory. Stop the Apache CouchDb service if it's running; copy couch_uuids.beam to the CouchDb binary directory at somewhere like
   $CouchDB\lib\couch-1.1.1\ebin, replacing the existing copy.
5. Edit $CouchDB\etc\couchdb\local.ini to add a line in the [couchdb] section along the lines of
   machine_id = 3
   (the value of machine_id must be different for each instance of CouchDb, and must be <= 255).
6. Restart the service, go into Futon, Configuration, and change the algorithm in the uuids section to utc_machine_id.