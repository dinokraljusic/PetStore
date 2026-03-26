# PetStore

Small demo project in .NET 10 representing a Pet Toy store.

The app uses MS SQL connector (thogh the same logic could be used for any provider e.g. SqlLite or MySQL).
On the first start, the DB is seeded with some initial data.

## Using the app

Pull this branch and do `Add-Migration` and `Update-Database`. Which will create `PetStoreDb` locally.
After the app starts, if the DB is empty, initial data seed will be called (in `DataSeed.cs`).

After the app starts, the Home page contains tle list of toys.
Build debug or Publish a release build to a local folder.
After you select toy(s) on the Home page, click `Buy`, and you'll be taken to the Create Purchase page where you'll be prompted to enter info as well as quantities.

```Server=localhost;Database=PetStoreDb;Trusted_Connection=True;TrustServerCertificate=True;```

<img width="440" height="308" alt="image" src="https://github.com/user-attachments/assets/d42bea86-4360-4c04-b4e8-9098ddf4a50d" />

_Pulish profile._

## Admin

To login as admin, click on the `Login` button on the top right.
The seeded creds are `admin@petstore.com` and `P@ssw0rd!`.
Once the login has succeded, you'll be able to access the Admin page from the Home page.
On the Admin page you can view purchases, toys, categories, etc.

## Notes

The project does not have any pictures for toys. A few approaches can be chosen for that depending on the scale:
 * Directly to the DB as byte[]: prop byte[] ImageData{get;set;} 
 * string link: public string? ImagePath {get;set;} like "/uploads/pproducts/toys/image12345.jpg" and keep the images locally or point to a web uri
 * Use object storage like Azure Blob or Amazon S3.
 * Users are created when a purches is made (email check if existing) based on the problem descriptio reading. Of course, users can be made on the same flow as Admin (modify Login to create) and create user non-admin type.
 * Admin privileges are handled by Identity.EntityFramework.
 
For a bigger project, we would:
 * have a separate table for price containing date applied and price history which would work on a DB trigger,
 * have deleted boolean and datetimeDeleted column, so as not to delete data on DELETE action, rather change status,
 * repository pattern could be used to make the controllers cleaner (since here the controllers handle all of business logic) and separate concerns,
 * disable Buy button when nothing selected,
 * use Clean architecture / onion so that it is more separated and testable: presentation calling service which uses domain entities...
