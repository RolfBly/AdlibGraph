# Readme

Original code by [BertDD](https://github.com/bertdd), slightly adapted to report objects not in used in a given Adlib application in 4 text files. 

In order to build this, you will need to add these proprietary dll's to the DDigitGraph subdirectory:

- Adlib.Constants.dll
- Adlib.Database.dll
- Adlib.Interfaces.dll
- Adlib.Objects.dll

These dll's come with Adlib Designer. Project is tested to work OK with dll's from Designer version 7.6.19234.1, the dll's themselves were version 1.7.19234. 

## Usage

Once built, from the command line: 

    adlibgraph.exe [parent application directory] [output_graph_filename.dgml]

## what it does 

Originally, AdlibGraph produces (yes!) a graph of all objects and their relations, in an Adlib application folder. More or less as a side effect, it also shows objects that aren't used at all in that particular application. You can view the graph in Visual Studio with the dgml viewer plugin. 

This is, however, not very useful. Even the simplest Adlib application has hundreds if not thousands of nodes and edges. Loading it takes a lot of time on any normal pc or laptop, and then you can't really do much with them. 

This is why I adapted the code to report objects that are not in use:

- screens (.fmt's) in `unused_screens.txt`
- databases (.inf's) in `unused_databases.txt`. 
- indexes in `unused_indexes.txt`. 
- fields in `unused_fields.txt`. 

The console output now also reports database that do not story anything, but are referred to as feedback database only. If you remove the INF, you have to make sure that none of the remaining databases mention them in de Feedback database section. 

## what it does not do (yet)

`program.cs` has two lines commented out. Uncommented, they tried to delete unused indexes and unused screens. At the time of this writing, _deleting_ "unused" screens is a bad idea. It's better to park them somewhere safe outside the main application directory. See todo. 

Deleting indexes in the original code does not work at all. You get a runtime error. Other than that, automatically removing indexes is a bad idea. See todo. 

## todo

- test and eliminate false positives and false negatives. Some of these only emerge when testing the application. 
- add command line switch to make generating the graph optional
- add command line switch to move unused screens elsewhere (default ..\[Applicatio folder name]_unused\screens)

### discussion

In theory, **removing unused indexes** might improve performance. Indexes (in a particular database) are updated every time a record is written. This may take some time. It may take less time when there are fewer indexes to update.  

The topic of removing unused fields requires further investigation. 





