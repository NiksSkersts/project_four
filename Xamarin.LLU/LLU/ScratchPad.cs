//To open files. One can use this.
foreach (var item in filepaths)
{
    Launcher.OpenAsync(new OpenFileRequest()
    {
        File = new ReadOnlyFile(item)
    });
}

protected override void OnCreate(Bundle savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    Xamarin.Essentials.Platform.Init(this, savedInstanceState);
}

//pievienot visām aktivitātēm
public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
{
    Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
}

string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
string path = Path.Combine(folder, "login");
