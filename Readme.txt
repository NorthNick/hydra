Setting up Hydra

1. Follow the instructions in Readme.txt in the CouchDb directory of the distribution to set up CouchDb.

2. Open the Hydra solution in the DotNet directory and compile the Messaging project.The DLLs created in its bin directory are the ones you will need to use Hydra in your projects. If you are going to use PubSubByType or Conversations, you will need to compile up those projects as well.

3. Try the PubSubByTypeExample project:
    a) Check that project's App.config file has the right settings for your Hydra database.
    b) Set the solution's startup project to PubSubByTypeExample and run it.

4. Try the conversatoin example:
    a) Check that the settings in the App.config files for ConversationExampleServer and ConversationExampleClient are correct.
    b) Set the solution's startup projects to be ConversationExampleServer and ConversationExampleClient.
    c) Run the solution.