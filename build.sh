#!/bin/bash
set -euo pipefail
dotnet run --project build/targets.csproj -- "$@"