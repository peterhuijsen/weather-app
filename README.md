### Weather applicatie
Voor het profielwerkstuk (PWS) is er naast een onderzoek ook een applicatie gemaakt, deze applicatie. Om ervoor te zorgen dat deze applicatie werkt moeten er een aantal dingen gebeuren:
1. De databank initialiseren
2. Het weermodel initialiseren
2. De server initialiseren
3. De website initialiseren

#### De databank
Voor de initialisatie van de databank moet er een docker container worden gemaakt waarvan de connection string in het configuratiebestand van de server moet worden gezet.
Dit configuratiebestand, `appsettings.json`, heeft ook andere instellingen die van belang zijn om te veranderen, waar we later aan toe komen.

#### Het weermodel
Voor het weermodel moeten een aantal pip packages worden geïnstalleerd:
1. PyTorch, nieuwste versie. Hiervoor kan je op hun website kijken.
2. Pandas, de nieuwste versie.
3. Scikit-learn, de nieuwste versie.
4. PyInstaller, de nieuwste versie.

Hierna kan het model gerund worden op een Apple Silicon macbook. Een andere architectuur vereist een verandering in het model.\
Hiervoor moet de waarde `mps` overal vervangen worden met `cuda` of met `cpu`.
Als laatste moet py

Na deze configuratie moet er een build worden gedaan worden met de volgende command:
```pyinstaller --onefile model.py```

#### De server
Voor de server moeten ook een aantal instellingen veranderen, met name het pad naar het model.
Hiervoor vraag ik u om contact met mij op te nemen zodat ik u dit op een veilige manier kan geven.

Hierna kan de server gerund worden met `dotnet run`.

#### 
Voor de client applicatie hoeven er geen instellingen te veranderen, er kan dan meteen `npm run dev` worden gerund.