#!/usr/bin/env bash
set -e

echo "Starting Ollama..."
ollama serve &

echo "Waiting for Ollama to be ready..."
sleep 5

echo "Pulling LLM model..."
ollama pull llama3:8b

echo "Starting ASP.NET API..."
exec dotnet CvEvaluator.Api.dll