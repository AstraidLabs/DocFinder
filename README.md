# DocFinder

DocFinder je desktopová WPF aplikace navržená pro bleskové vyhledávání a přehlednou správu lokálních dokumentů. Projekt vznikl jako reakce na omezení standardního vyhledávání ve Windows: pomalé indexování, chybějící práce s metadaty a nemožnost snadno přidávat další formáty. Cílem je nabídnout nástroj, který dokáže dokumenty inteligentně načíst, roztřídit a zpřístupnit na pár stisků kláves.

## Motivace
- rychle najít smlouvy, technické dokumenty či e‑maily v osobním i firemním archivu,
- eliminovat duplicitní dokumenty a udržet pořádek v projektech,
- vytvořit otevřenou platformu pro snadné rozšiřování o další extraktory a typy souborů.

## Funkcionalita
- **Automatická indexace** sledovaných složek a detekce změn pomocí `WatcherService`.
- **Extrakce obsahu** z formátů jako DOCX a PDF, včetně autora, verze a dalších metadat.
- **Fulltextové vyhledávání** postavené na Lucene.NET s podporou fuzzy dotazů, filtrováním a řazením výsledků.
- **Překryvné vyhledávací okno** dostupné z klávesnice pro okamžitý přístup k dokumentům.
- **Perzistentní katalog** v SQLite a komprimované ukládání obsahu pro úsporu místa.
- **Nastavení v JSON** (`%LOCALAPPDATA%/DocFinder/settings.json`) s automatickým doplněním výchozích hodnot.

## Opravy a zlepšení
- Indexer bezpečně reaguje na odstranění nebo přesun souborů a synchronně aktualizuje katalog i vyhledávací index.
- Ošetření chyb I/O a nedostatečných oprávnění zabraňuje pádům služby při práci se soubory.
- Průběžné commitování a komprese obsahu udržují Lucene index v konzistentním stavu a zlepšují výkon.
- Systém nastavení zapisuje změny pouze při volání `SaveAsync`, čímž se předchází nekonzistencím.

## Budoucí využití
DocFinder je vhodný pro právní kanceláře, technické týmy i domácí archivy – všude tam, kde je potřeba rychle vyhledávat v rozsáhlých složkách dokumentů. Po přidání sledovaných cest běží indexace na pozadí a výsledky jsou dostupné okamžitě.

## Vývoj a testování
Repozitář obsahuje projektovou strukturu pro .NET 8 a sadu jednotkových testů (xUnit). Testy je možné spustit příkazem:

```bash
 dotnet test
```

