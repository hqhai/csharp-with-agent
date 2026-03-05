#!/bin/bash

function base_installation() {
    apt-get update && apt-get install -y \
    libgdiplus \
    libgtk-3-0 \
    libnss3 \
    libappindicator3-1 \
    libasound2 \
    libgbm1 \
    libnspr4 \
    libnss3 \
    xdg-utils \
    libx11-xcb1 \
    libxcomposite1 \
    libxrandr2 \
    libxdamage1 \
    libxcursor1 \
    libwayland-client0 \
    libwayland-server0 \
    libxi6 \
    libxtst6
}

function setup_font() {
    sed -i 's/^deb http:\/\/deb.debian.org\/debian bullseye main$/deb http:\/\/deb.debian.org\/debian bullseye main contrib non-free/' /etc/apt/sources.list
    sed -i 's/^deb http:\/\/deb.debian.org\/debian bullseye-updates main$/deb http:\/\/deb.debian.org\/debian bullseye-updates main contrib non-free/' /etc/apt/sources.list
    sed -i 's/^deb http:\/\/deb.debian.org\/debian-security bullseye-security main$/deb http:\/\/deb.debian.org\/debian-security bullseye-security main contrib non-free/' /etc/apt/sources.list
    apt-get update
}

function install_font_arial() {
    apt install -y ttf-mscorefonts-installer
}

function keep_font_arial() {
    cd /usr/share/fonts/truetype/msttcorefonts
    ls | grep -v -E 'arial|Arial' | xargs -d '\n' rm -rf --
}

function cleanup() {
    apt-get clean
    rm -rf /var/lib/apt/lists/*
}

base_installation
#setup_font
# install_font_arial
# keep_font_arial
cleanup
