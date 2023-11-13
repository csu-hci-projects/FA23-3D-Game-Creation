1. Problema

Ent�o queremos gerar aleat�riamente um mapa de salas. Eu tinha originalmente pensado adaptar um dos videos que o Manuel enviou, que mostrava como gerar salas
tipo Binding of Isaac. O catch � que em vez de uma grid 2D seria 3D, sendo possivel dar stack de rooms. Logo numa room podias ir a um portal que passava-te para
uma room acima. O objetivo original era dividir a tracking area em x peda�os do tamanho da sala, e isso determinaria o (x,y) da grid com z gigantesco.
Mas devido � tracking area pequena, que � basicamente do tamanho da sala inteira, e para demonstrar overlap, agora as salas t�m todas overlap 100%.

Logo como a posi��o relativa das salas j� n�o � relevante (como n�o tens v�rias salas na tracking area, nunca vais de uma sala para outra com esse movimento
a corresponder minimamente com a realidade), n�o � preciso uma grid/mapa convencional.

2. Solu��o

Dai estar tudo numa �rvore. Tens uma raiz, e cada sala guarda para que sala teletransporta. De notar que um teleporter teletransporta-te para a mesma posi��o s�
que na outra sala. Quando criamos a raiz, damos logo spawn dos filhos. Quando mudamos de sala, criamos os filho sdessa nova sala. Assim, teoreticamente, podes
ter um mapa infinito (h� c�digo para limitar a produnfidade da �rvore, porque temos que garantir que funciona em pequena escala).

Ent�o s� para dar overview das classes:

TreeNode � a �rvore. � muito semelhante �s que trabalhamos em java, em principio n�o tens que tocar nada

Teleporter � para meter no portal. Tem uma RoomDir, um enum definido em RoomDirections (j� explico em baixo), que diz onde na sala est� esse portal. Isto � 
importante, porque como dito, um portal na parte norta da sala, teletransporta-te para a parte norte de outra sala e tens que ver que salas ligam.
No OnTriggerEnter, no caso de ser o Player, tamb�m chama fun��es do GenerationManager para criar os filhos e ir desativando salas desnecess�rias.
Tamb�m guarda a sala onde est� o portal e para onde vai.

RoomDirections � para meter em cada tipo de sala (prefab) que vais passar ao gerador. Tem um enum com as varias dire��es onde est�o os portais (e supostamente 
faltam mais). Est� ai dentro da classe uma lista para preenchermos no inspector, para metermos todas as dire��es/saidas possiveis da sala. 
Apesar de ter dito que � para preenchermos manualmente no inspetor, se calhar com a fun��o Awake, podemos simplesmente percorrer os portais (visto que s�o 
objetos filhos da sala) e sacar as dire��es deles.

Room � o tipo de node na �rvore. Guarda a sala instanciada e as suas directions. 

Finalmente temos o Generation Manager, a grande classe. Eu, como em tudo, meti coment�rios que devem explicar tudo bem quando olhas para o c�digo, logo n�o
vou entrar em especificos aqui. Esta classe � static, e um singleton meio half-assed, para as outras classes, como o Teleporter a poderem aceder facilmente.

Basicamente vais-lhe passar duas listas de prefabs, salas e salas finais (salas que s� t�m uma direction, logo s�o becos sem sa�da). No inicio, primeiro
ele vai logo buscar as dire�oes a todos os prefabs e guarda em numa lista no mesmo tamanho que as listas de prefabs. Isto � para limitar o uso de 
GetComponent mais � frente. Depois  uma sala ao calhas, d�-lhe spawn, mete na �rvore e d� spawn do player. E se a depth m�xima n�o for 1, 
ele cria logo os filhos tamb�m.

Logo em baixo est� OnPortalPass a fun��o que � chamada quando passas num portal. Recebe a sala em que acabas depois do teleport. Quando aterras numa sala
ele cria logo os filhos, ou seja, as pr�ximas salas a que podes ir parar. Adicionalmente ele chama o m�todo GarbageCleanup. Basicamente ele ve as salas que 
n�o est�o imediatamente a seguir � que tu vais e desativa-as para reduzir o que o Unity est� a dar render.

A seguir � o m�todo de dar spawn das crian�as. Precisas de para cada direction saida (menos a que tu entraste), criar um filho. Logo ves se � uma sala 
ou beco sem saida, escolhes um ao calhas (h� m�todos auxilaires para isso) e vais ver a nova posi��o onde dar spawn (j� falo disso) e d�s instantiate, 
vais procurar o portal certo em cada sala para dar set da origem e destino nos dois portais (sala pai/currente e sala filho), e metes
na �rvore.

H� v�rios m�todos auxiliares, para escolher sala ou calhas ou ir buscar a nova posi��o. Falando disso, h� uma lista de Vector2 que s�o as posi��es. A raiz est�
em (0,0). Mas isso n�o se traduz em espa�o real, porque as salas s�o tipo 6x6 unidades, logo tamb�m h� uma vari�vel que � o tamanho da grid que est� a 10 por 
agora. No GetNewPosition() tens que ir buscar o proximo indice e converter para Vector3, tal como quando metes uma sala na �rvore, tens que guardar o Vector2
correspondente a esta grid falsa. No m�todo est�o l� 2 sugest�es para implementar este m�todo.

O c�digo para al�m de estar bastante comentado, tem bastantes TODO, coisas a fazer, ver no futuro, ou optimizar.

3. O que fazer (Os TODO s�o todos no GenerationManager, mas podes precisar de mudar outras classes)

Para por a funcionar:

Implementar o GetNewPosition (TODO linha 233)

Depois podes criar prefabs, preencher as dire��es nas salas e portais, possivelmente colocando novas no enum, e j� podes testar.

Em adi��o n�o fiz isso ainda, mas como salas guardam dire��es, em principio v�o ter que guardar se t�m gelo. Ou seja, tratar essa boolean gelo como tratas as 
dire�oes (ir buscar no Awake e quando escolhes a sala ver se o filho tamb�m vai ter gelo).

De resto falta meter ifs e guardas em v�rios sitios em que o spawn ou assim pode n�o dar e algo d� null, e mini optimiza��es que est�o a� nos TODO para
reduzir o uso de GetComponent
