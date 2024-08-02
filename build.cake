var target = Argument("target", "Publish");

Task("Publish")
    .Does(() => 
    {
        Information("Cleaning old release files!");

        DotNetClean("./", new DotNetCleanSettings 
        {
            Configuration = "Release"
        });

        Information("Trying to publish Tunnelize.Server application!");

        DotNetPublish("./", new DotNetPublishSettings 
        {
            Configuration = "Release"
        });

        Information("Trying to publish Tunnelize.Client osx-x64 application!");

        DotNetPublish("./src/Tunnelize.Client", new DotNetPublishSettings 
        {
            Configuration = "Release",
            Runtime = "osx-x64"
        });

        Information("Trying to publish Tunnelize.Client win-x64 application!");

        DotNetPublish("./src/Tunnelize.Client", new DotNetPublishSettings 
        {
            Configuration = "Release",
            SelfContained = true,
            Runtime = "win-x64"
        });

        Information("Trying to publish Tunnelize.Client linux-x64 application!");

        DotNetPublish("./src/Tunnelize.Client", new DotNetPublishSettings 
        {
            Configuration = "Release",
            SelfContained = true,
            Runtime = "linux-x64"
        });


        Information("Publish done!");
    });

 RunTarget(target);

