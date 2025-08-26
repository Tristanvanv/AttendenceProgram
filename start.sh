#!/usr/bin/env sh
# Render zet $PORT op runtime. Laat Kestrel daarop luisteren.
export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-10000}"
exec dotnet AttendenceProgram.dll
