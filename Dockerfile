FROM mono:4.2.2.30

MAINTAINER cp-nilly <cp.nilly@gmail.com>

COPY . /usr/src/app/source
WORKDIR /usr/src/app/source
RUN mkdir -p /usr/src/app/source /usr/src/app/build \
    && nuget restore -NonInteractive \
    && xbuild BuildDocker.proj \
    && chmod 755 docker-entrypoint.sh \
    && mv docker-entrypoint.sh /usr/src/app/build/docker-entrypoint.sh \
    && rm -rf /usr/src/app/source
WORKDIR /usr/src/app/build
EXPOSE 80 2051
ENTRYPOINT ./docker-entrypoint.sh