# OwlDocs
Simple documentation application. OwlDocs provides an easy way to store, edit, and maintain documentation. It is a self-hosted solution that allows users to create and edit markdown files in the browser, backed by an ASP.NET Core backend. 

## Data Providers
OwlDocs exposes two document providers: Sqlite database, or native File System. By changing setting the appsettings.json, you can point OwlDocs at an existing documentation folder, or create a local sqlite database to store all your documentation.

## File Types
OwlDocs works uses Markdown. You can create new files and edit the markdown in your browser. The markdown is then converted to html and stored in the sqlite database, or converted at runtime when using the filesystem.  You can also upload images and reference them within your markdown. Accepted image types is configurable using appsettings.json.
