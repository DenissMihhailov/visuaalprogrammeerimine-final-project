SEKVENTSER

Enne esimest käivitamist:
Rakenduse korrektseks tööks tuleb enne esmakordset käivitamist initsialiseerida andmebaas. Selleks ava projekti kaustas terminal (PowerShell või muu käsurida) ja käivita järgmised käsud:

dotnet ef migrations add UpdateSeedPattern -p App.Infrastructure -s RhythmMachineUI
dotnet ef database update -p App.Infrastructure -s RhythmMachineUI

Need käsud loovad või uuendavad vajaliku andmebaasiskeemi ning lisavad algandmed (seed). Ilma selle sammuta ei pruugi rakenduses olla ühtegi mustrit ega heli, mida kasutada.


See on väike step-sekventser, mille ma tegin selleks, et biite ja rütme kiiresti kokku panna. Idee oli teha midagi lihtsat ja otsest, ilma liigse menüüde ja seadistusteta.

Sekventser töötab 16 sammu peal ja seda saab kasutada reaalajas, samal ajal kui rütm mängib, saab samme sisse ja välja lülitada, BPM-i muuta ja helisid vahetada. Kõik reageerib kohe, ilma et peaks playback’i peatama.

Rakenduses on valmis helipakid (kitid), nii et saab kohe alustada ja keskenduda rütmile, mitte soundide otsimisele. Mustreid ja kite saab salvestada ning hiljem uuesti kasutada.

See on mõeldud nii ideede visandamiseks, harjutamiseks kui ka lihtsalt jam’iks, kui tahad midagi kiirelt käima panna ja vaadata, kuhu see viib.
