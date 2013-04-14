Modifying CouchDb for use with Hydra version below 1.3
------------------------------------------------------

NOTE: These changes are included in versions of CouchDb from 1.3 onwards so you do not need to take any action with those versions.

Hydra requires document ids that are monotonically increasing for any CouchDb instance, and globally unique across instances. The files here implement a document id
scheme with these properties, by modifying the couch_uuids.erl file in the CouchDb source at git://git.apache.org/couchdb.git.

Files
-----

couch_db.hrl		- include file needed for compilation. Taken directly from the CouchDb source.
couch_uuids.erl		- modified version of couch_uuids.erl
couch_uuids.src.erl - original version, taken from the CouchDb source.


Installation
------------

To modify CouchDb, do as follows. Here $CouchDb refers to the CouchDb installation directory, usually C:\Program Files (x86)\Apache Software Foundation\CouchDB

1. Open a command prompt and change to the directory containing this Readme.txt file.
2. Run the Erlang werl.exe program in that command window. This is found in the CouchDb installation tree, at something like $CouchDB\erts-5.9\bin\werl.exe.
3. In the resulting window, type
   c(couch_uuids).
   including the final dot, and then hit return. That should yield {ok,couch_uuids}. Type halt().<return> to exit Erlang.
4. You should now have a couch_uuids.beam file in this directory. Stop the Apache CouchDb service if it's running; copy couch_uuids.beam to the CouchDb binary directory at somewhere like
   $CouchDB\lib\couch-1.2.0\ebin, replacing the existing copy.
5. Restart the service. Now configure CouchDb as described in the README in the directory above this one.
