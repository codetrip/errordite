#This is the report sed script only.  The fix script is in FixUp_Fix.sed.
#NOTE: ensure the commands in this file are suffixed with p (to ensure they produce output)
#NOTE: but ensure the commands in FixUp_Fix do NOT have the suffix (or the line will output twice)

#this command looks for references to Asos binaries that do not use the $(Configuration) variable
s/\(.*\\lib\\Errordite\\\)\([Debug|Release]\)\(.*\)/Rule 1:&/p

#this command looks for any absolute references to files in the References folder and replaces it with relative
s/\(.*\).:\\.*\(\\lib\\.*\)/Rule 2:&/p

#this command looks for Asos references to a bin folder
s/.*<HintPath>.*bin\\Debug\\.*/Rule 3:&/p

s/.*<Reference Include="\(Errordite.*\), Version.*/Rule 4:&/p