CouchDb setup
-------------

Install the latest version of CouchDb: get the installer from couchdb.apache.org. Install.

If your CouchDb is below Version 1.3, then modify Erlang as described in Erlang\Readme.txt. (Later versions do not need modification, as the Erlang changes have been incorporated in the CouchDb distribution. Using Version 1.3 or above is strongly recommended.)
If this is the first instance of CouchDb, create the Hydra database as below. Otherwise go to the config step.

1. In Futon create an admin logon with username and password of your choice. Note that Futon may misbehave in browsers other than Firefox, so it is safest to use that.
2. Log in as your admin user.
3. Click Create database and enter a name for your Hydra database. Example programs in the Hydra distribution assume the database is called hydra, but this can be changed in app.config.
4. Go into your new database and create design documents as follows:
   Click New document, then click the Source box on the resulting page.
   Replace the text with the contents of the validate.json file from the _design subdirectory of this directory.
   Click Save Document.
   Repeat with the contents of the hydra.json and filters.json documents.

Config
1. In Futon, go to Configuration.
2. Set bind_address=0.0.0.0, max_replication_retry_count=infinity, delayed_commits=false. You may also want to set level=error in the log section, to prevent your log becoming too huge.
3. Set algorithm=utc_id. At the bottom of the Futon page, click "Add a new section"; in the resulting dialogue box, set section=uuids, option=utc_id_suffix, value=<suffix> where <suffix> is a string of your choice, unique to this CouchDb instance. (You might choose a number e.g. 23, or the machine name, or any unique string you fancy. But bear in mind that these strings occur in every message and in all the database indexes, so long suffixes can use up a lot of space.) Click the Create button.
4. If you are installing on Windows 2008 Server, then be aware that the default file compression technique, snappy, does not work properly as of CouchDb version 1.2 - see https://issues.apache.org/jira/browse/COUCHDB-1482. You can change file_compression on these machines to something like deflate_6 to get working file compression. Note that different instances can specify different compression settings with no harm. This problem  is fixed in Version 1.2.1 and above.

Replication
If this is not the first instance of CouchDb, then set up replication.
1. In Futon go into the _replicator database.
2. For each machine, <machine_name> with which you want to replicate:
   a. Create a document in _replicator called PullFrom<machine-name>
   b. Give it fields create_target=true, continuous=true, source=http://<user>:<password>@<machine_name>:5984/hydra, target=http://<user>:<password>@127.0.0.1:5984/hydra
      <user> and <password> are those for your CouchDb admin logon. It's probably best to make the <machine_name> in source a FQDN to prevent ambiguity.
   c. Save the document. Check that it gets modified to say replication_state=triggered.
3. For each of the replication targets above, create an equivalent pull replication in its _replicator database. Fields will be identical except that the machine name in source will be the machine on which you're installing CouchDb.

Compaction
If you are using CouchDb 1.2 or above, you can set up regular compaction. This is a new feature and it's not yet clear which settings work best. They will also depend on the volume of Hydra traffic in your database, so you should experiment.
1. In Futon, go to Configuration
2. Click the "Add a new section" link at the bottom of the page.
3. Set section=compactions, option=hydra (or whatever name you are using for your hydra database), and
value=[{db_fragmentation, "70%"}, {view_fragmentation, "60%"}, {from, "23:00"}, {to, "04:00"}]
or something similar, where the fragmentation values are percentages of old data and old view index data in the database, and from and to are times of day between which compaction should take place.