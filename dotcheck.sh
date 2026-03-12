#!/bin/bash
dotnet build \
  --no-restore \
  -p:ProduceOnlyReferenceAssembly=true \
  -p:GenerateDocumentationFile=false \
  -p:BaseIntermediateOutputPath=obj_check/ \
  -p:BaseOutputPath=bin_check/ \
  -tl
