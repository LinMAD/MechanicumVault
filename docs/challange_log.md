# Session Log

Simple personal activity journal that is appendix for fun :)

## 02.11.24
- 20:24 (New session) Work with design document.

## 03.11.24
- 00:23 (End of session) Make a draft architecture of application with a project setup.
- 09:12 (New session) Design and create file system watcher with N implementations, idea is that I could add Google Drive provider later.
- 12:18 Implementation done, covered FileSynchronizationProvider with Unit and Functional tests.
- 13:40 (End of session) Integrate to client and server application FileSynchronizationProvider.

## 04.11.24
- 11:57 (New session) Implement TCP Transport wrapper that is used for server connection, update related code to use it.
- 12:20 (End of session) Break 
- 13:14 (New session) Work more on server side and unify TCP transport between Client and Server.
- 14:40 Add TCP transports for Client and Server, connect them
- 17:54 Implement logic that allows sync file from client to server, there's few issues that I need to solve
  - Sub folder sync, multiple files, rename of file
- 17:55 (End of session) Break 
- 19:08 (New session) Rewrite FileSynchronizationMessage to use MessagePackObject package that is more binary safe compare to Json.
- 20:36 Finish Prof Of Concept that can synchronize files between Client and Server.
- 22:06 Finish draft architecture notes, charts and document it in README.md
