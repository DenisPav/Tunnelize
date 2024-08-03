var target = Argument("target", "Publish");

Task("Publish")
    .Does(() => 
    {
        Information("Cleaning old release files!");

        DotNetClean("./", new DotNetCleanSettings 
        {
            Configuration = "Release"
        });

        Information("Trying to publish Tunnelize applications!");

        DotNetPublish("./", new DotNetPublishSettings 
        {
            Configuration = "Release"
        });

        Information("Publish done!");
    });

 RunTarget(target);

