#This is the fix sed script.  
#Remember to add any updates or additions to FixUp_Report.sed so we can see what will be fixed

#NOTE: ensure the commands in this file are NOT suffixed with p (to ensure they produce output)
#NOTE: but ensure the commands in FixUp_Report have the p suffix (or the line will output twice)

#this command looks for references to Asos.Marketplace binaries that do not use the $(Configuration) variable
s/\(.*\\lib\\Errordite\\\)\(Debug\|Release\)\(.*\)/\1$(Configuration)\3/

#this command looks for any absolute references to files in the References folder and replaces it with relative
s/\(.*\).:\\.*\(\\lib\\.*\)/\1..\\..\2/

#this command looks for Asos.Marketplace references to a bin folder
s/\(.*<HintPath>\).*bin\\Debug\\\(.*\)/\1..\\..\\lib\\Errordite\\$(Configuration)\\\2/

s/\(.*<Reference Include="Errordite.*\), Version.*\(".*>\)/\1\2/