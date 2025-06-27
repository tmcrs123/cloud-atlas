#!/bin/sh
echo "bananas" && sleep 3600 && /opt/mssql-tools/bin/sqlcmd -S \"$SQL_HOST\" -U \"$SQL_USER\" -P \"SQL_PASSWORD\" -i /tmp/seed.sql && echo "Done."
