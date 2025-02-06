# Nevio's Docker Dokumentation #
INF2023I
M347
V1
![Alt text](image.png "Docker logo (Cute Whales)")

<details>
<summary>Für was sind Docker-Container nützlich? S.1</summary>
<h3>Für was sind Docker-Container nützlich?</h3>
Docker-Container sind nützlich, weil sie eine isolierte, konsistente Umgebung bieten, die es Entwicklern und Systemadministratoren ermöglicht, Anwendungen unabhängig von der zugrunde liegenden Infrastruktur auszuführen. Hier sind einige Hauptvorteile und Einsatzmöglichkeiten von Docker-Containern:

- Portabilität
- Konsistenz zwischen Entwicklungs-, Test- und Produktionsumgebungen
- Skalierbarkeit und Flexibilität
- Ressourcen-Effizienz
</details>
<details>
<summary>Was ist Dev-Ops? S.2</summary>
<h3>Was ist Dev-Ops?</h3>
DevOps ist eine Abkürzung für "Development" (Entwicklung) und "Operations" (Betrieb). Es handelt sich um eine kulturelle und praktische Herangehensweise an Softwareentwicklung und IT-Betrieb, die darauf abzielt, die Zusammenarbeit zwischen Entwicklern (die neue Funktionen entwickeln) und Operations-Teams (die für die Bereitstellung und den reibungslosen Betrieb der Software verantwortlich sind) zu verbessern.
</details>
<details>
<summary>Unterschied Virtualisierung vs. Containerisierung S.3</summary>
<h3>Unterschied Virtualisierung vs. Containerisierung</h3>

- Virtualisierung: Hier werden ganze virtuelle Maschinen (VMs) erstellt, die eine komplette Betriebssysteminstanz und Anwendungen beinhalten. Jede VM nutzt eine eigene Betriebssysteminstanz und Ressourcen.

- Containerisierung: Container teilen sich das Betriebssystem des Hosts und isolieren Anwendungen und deren Abhängigkeiten voneinander. Sie sind leichtgewichtiger als VMs und starten schneller, da sie den Overhead einer vollständigen Betriebssysteminstanz vermeiden.

Containerisierung, insbesondere durch Docker, hat die Bereitstellung von Anwendungen vereinfacht und die Effizienz in der Cloud-Computing-Welt erheblich verbessert.
</details>

>[!NOTE]
>Die oberen Texte wurden von ChatGPT geschrieben ich habe recherchiert ob alles stimmt und da ich fand das er es kurz und knackig erklärt habe ich seine Erklärung verwendet.
<details>
<summary>Docker Login S.4</summary>
<h3>Docker Login</h3>
Ich musste mich nicht registrieren da ich bereits ein Konto hatte also konnte ich mich 
einfach via Google einloggen.

![Alt text](docker-login-page.png "Login Screen")

Dannach war ich bereits auf meinem Konto eingelogged.

![Alt text](docker-logged-in.png "Logged In Screen")
</details>
<details>
<summary>Unterschied Container und Image S.5</summary>
<h3>Unterschied Container und Image</h3>

  - Docker-Image: Ein Image ist wie eine Vorlage, die verwendet wird, um Container zu erstellen. Die Anweisungen zum Erstellen eines Docker-Containers enthält. Ein Image enthält alles, was notwendig ist, um eine Anwendung auszuführen – wie Code, Laufzeitumgebung, Bibliotheken, Umgebungsvariablen und Konfigurationsdateien.
<br></br>
- Docker-Container: Ein Container ist eine laufende Instanz eines Images. Er erstellt das benötigte Environment um eine Anwendung und ihre Abhängigkeiten zu erfüllen dies so resourcensparend wie möglich. Container, die aus demselben Image erstellt werden, sind hinsichtlich ihrer Konfiguration und ihres Verhaltens identisch.

Kurz gesagt: Ein Image ist die Vorlage für den Container
</details>
