# Release Notes

## Version 2.3.3

- Added more data types to the ParseValue method

## Version 2.3.2

- Fixed issue where columns with type INTEGER would return unknown instead of the actual value

## Version 2.3.1

- Fixed issue where table view would crash if values were null
- Introduced better way to parse values based on column type

## Version 2.3.0

- Added error message to indicate no column names are found
- Added TRUNCATE function to table view
- Added DROP function to table view
- Added TRUNCATE/DROP all tables function to index view

## Version 2.2.3

- Added check to only run when debugger is attached

## Version 2.2.1

- Added GitHub Actions workflow
- Add new row button is hidden when no data exists (because no columns can be loaded)

## Version 2.2.0

- Added prepared statements for safer queries

## Version 2.1.1

- Fixed styling not being applied

## Version 2.1.0

- Updated styling

## Version 2.0.0

- Moved to custom SQLite wrapper
- Removed System.Data.SQLite dependency
- Added compatibility with all MAUI build platforms

## Version 1.2.1

- Added README and LICENSE

## Version 1.2.0

- Initial release with basic functionality
