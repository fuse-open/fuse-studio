# Restore package.json.
mv -f package.json-original package.json

# Restore binaries.
node scripts/restore.js
