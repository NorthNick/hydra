CouchDb setup
-------------

First modify Erlang as described in Erlang\Readme.txt

If this is the first instance of CouchDb, create the Hydra database as below. Otherwise go to the config step.

1. In Futon create an admin logon with the standard username and password (currently admin/match)
2. Create a new database called Hydra.
3. Add the design documents in the _design directory to the Hydra database.

Config
1. In Futon, go to Configuration.
2. Set bind_address=0.0.0.0, delayed_commits=false, max_retry_count=infinity
3. Ensure that algorithm=utc_machine_id (this should have been done during the Erlang changes above).
4. Ensure that machine_id is a unique value across all CouchDb instances (see the Erlang changes for a bit more on this).

Replication
If this is not the first instance of CouchDb, then set up replication.
1. In Futon go into the _replicator database.
2. For each machine, <machine_name> with which you want to replicate:
   a. Create a document in _replicator called PullFrom<machine-name>
   b. Give it fields create_target=true, continuous=true, source=http://admin:match@<machine_name>:5984/hydra, target=http://admin:match@127.0.0.1:5984/hydra
      It's probably best to make the <machine_name> in source a FQDN to prevent ambiguity.
   c. Save the document. Check that it gets modified to say replication_state=triggered.
3. For each of the replication targets above, create an equivalent pull replication in its _replicator database. Fileds will be identical except that the machine name
   in source will be the machine on which you're installing CouchDb.