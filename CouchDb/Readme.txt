CouchDb setup
-------------

Install CouchDb Version 1.2: get the installer from couchdb.apache.org. Install.

Then modify Erlang as described in Erlang\Readme.txt

If this is the first instance of CouchDb, create the Hydra database as below. Otherwise go to the config step.

1. In Futon create an admin logon with the standard username and password (admin/match in all the Hydra examples)
2. Log in as your admin user.
3. Click Create database and enter a name for your Hydra database. Example programs in the Hydra distribution assume the database is called hydra, but this can be changed in app.config.
4. Go into your new database and create design documents as follows:
   Click New document, then click the Source box on the resulting page.
   Replace the text with the contents of the validate.json file from the CouchDb project in the Hydra distribution.
   Click Save document.
   Repeat with the contents of the hydra.json document.

Config
1. In Futon, go to Configuration.
2. Set bind_address=0.0.0.0, max_replication_retry_count=infinity, delayed_commits=false.
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
3. For each of the replication targets above, create an equivalent pull replication in its _replicator database. Fields will be identical except that the machine name in source will be the machine on which you're installing CouchDb.

Compaction
Set up regular compaction. This is a new feature in CouchDb 1.2 and it's not yet clear which setting work best. They will also depend on the volume of Hydra traffic in your database, so you should experiment.
1. In Futon, go to Configuration
2. Click the "Add a new section" link at the bottom of the page.
3. Set section=compaction, option=hydra (or whatever name you are using for your hydra database), and
value=[{db_fragmentation, "70%"}, {view_fragmentation, "60%"}, {from, "23:00"}, {to, "04:00"}]
or something similar, where the fragmentation values are percentages of old data and old view index data in the database, and from and to are times of day between which compaction should take place.