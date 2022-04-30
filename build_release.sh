#!/bin/bash

nuget restore
# Options Debug|Release
CONFIGURATION=Release
rm -rf $CONFIGURATION
msbuild /t:ResxTranslator /property:Configuration=$CONFIGURATION /p:OutputPath=../$CONFIGURATION/lib/ResxTranslator
mkdir -p ./$CONFIGURATION/bin
echo "#!/bin/sh
SCRIPT_DIR=\$( cd -- \"\$( dirname -- \"\${BASH_SOURCE[0]}\" )\" &> /dev/null && pwd )
exec mono \$SCRIPT_DIR/../lib/ResxTranslator/ResxTranslator.exe \"\$@\"" >> ./$CONFIGURATION/bin/resx_translator
chmod +x ./$CONFIGURATION/bin/resx_translator
