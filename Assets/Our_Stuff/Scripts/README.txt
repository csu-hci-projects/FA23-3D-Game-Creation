1. Problema

Então queremos gerar aleatóriamente um mapa de salas. Eu tinha originalmente pensado adaptar um dos videos que o Manuel enviou, que mostrava como gerar salas
tipo Binding of Isaac. O catch é que em vez de uma grid 2D seria 3D, sendo possivel dar stack de rooms. Logo numa room podias ir a um portal que passava-te para
uma room acima. O objetivo original era dividir a tracking area em x pedaços do tamanho da sala, e isso determinaria o (x,y) da grid com z gigantesco.
Mas devido à tracking area pequena, que é basicamente do tamanho da sala inteira, e para demonstrar overlap, agora as salas têm todas overlap 100%.

Logo como a posição relativa das salas já não é relevante (como não tens várias salas na tracking area, nunca vais de uma sala para outra com esse movimento
a corresponder minimamente com a realidade), não é preciso uma grid/mapa convencional.

2. Solução

Dai estar tudo numa árvore. Tens uma raiz, e cada sala guarda para que sala teletransporta. De notar que um teleporter teletransporta-te para a mesma posição só
que na outra sala. Quando criamos a raiz, damos logo spawn dos filhos. Quando mudamos de sala, criamos os filho sdessa nova sala. Assim, teoreticamente, podes
ter um mapa infinito (há código para limitar a produnfidade da árvore, porque temos que garantir que funciona em pequena escala).

Então só para dar overview das classes:

TreeNode é a árvore. É muito semelhante às que trabalhamos em java, em principio não tens que tocar nada

Teleporter é para meter no portal. Tem uma RoomDir, um enum definido em RoomDirections (já explico em baixo), que diz onde na sala está esse portal. Isto é 
importante, porque como dito, um portal na parte norta da sala, teletransporta-te para a parte norte de outra sala e tens que ver que salas ligam.
No OnTriggerEnter, no caso de ser o Player, também chama funções do GenerationManager para criar os filhos e ir desativando salas desnecessárias.
Também guarda a sala onde está o portal e para onde vai.

RoomDirections é para meter em cada tipo de sala (prefab) que vais passar ao gerador. Tem um enum com as varias direções onde estão os portais (e supostamente 
faltam mais). Está ai dentro da classe uma lista para preenchermos no inspector, para metermos todas as direções/saidas possiveis da sala. 
Apesar de ter dito que é para preenchermos manualmente no inspetor, se calhar com a função Awake, podemos simplesmente percorrer os portais (visto que são 
objetos filhos da sala) e sacar as direções deles.

Room é o tipo de node na árvore. Guarda a sala instanciada e as suas directions. 

Finalmente temos o Generation Manager, a grande classe. Eu, como em tudo, meti comentários que devem explicar tudo bem quando olhas para o código, logo não
vou entrar em especificos aqui. Esta classe é static, e um singleton meio half-assed, para as outras classes, como o Teleporter a poderem aceder facilmente.

Basicamente vais-lhe passar duas listas de prefabs, salas e salas finais (salas que só têm uma direction, logo são becos sem saída). No inicio, primeiro
ele vai logo buscar as direçoes a todos os prefabs e guarda em numa lista no mesmo tamanho que as listas de prefabs. Isto é para limitar o uso de 
GetComponent mais à frente. Depois  uma sala ao calhas, dá-lhe spawn, mete na árvore e dá spawn do player. E se a depth máxima não for 1, 
ele cria logo os filhos também.

Logo em baixo está OnPortalPass a função que é chamada quando passas num portal. Recebe a sala em que acabas depois do teleport. Quando aterras numa sala
ele cria logo os filhos, ou seja, as próximas salas a que podes ir parar. Adicionalmente ele chama o método GarbageCleanup. Basicamente ele ve as salas que 
não estão imediatamente a seguir à que tu vais e desativa-as para reduzir o que o Unity está a dar render.

A seguir é o método de dar spawn das crianças. Precisas de para cada direction saida (menos a que tu entraste), criar um filho. Logo ves se é uma sala 
ou beco sem saida, escolhes um ao calhas (há métodos auxilaires para isso) e vais ver a nova posição onde dar spawn (já falo disso) e dás instantiate, 
vais procurar o portal certo em cada sala para dar set da origem e destino nos dois portais (sala pai/currente e sala filho), e metes
na árvore.

Há vários métodos auxiliares, para escolher sala ou calhas ou ir buscar a nova posição. Falando disso, há uma lista de Vector2 que são as posições. A raiz está
em (0,0). Mas isso não se traduz em espaço real, porque as salas são tipo 6x6 unidades, logo também há uma variável que é o tamanho da grid que está a 10 por 
agora. No GetNewPosition() tens que ir buscar o proximo indice e converter para Vector3, tal como quando metes uma sala na árvore, tens que guardar o Vector2
correspondente a esta grid falsa. No método estão lá 2 sugestões para implementar este método.

O código para além de estar bastante comentado, tem bastantes TODO, coisas a fazer, ver no futuro, ou optimizar.

3. O que fazer (Os TODO são todos no GenerationManager, mas podes precisar de mudar outras classes)

Para por a funcionar:

Implementar o GetNewPosition (TODO linha 233)

Depois podes criar prefabs, preencher as direções nas salas e portais, possivelmente colocando novas no enum, e já podes testar.

Em adição não fiz isso ainda, mas como salas guardam direções, em principio vão ter que guardar se têm gelo. Ou seja, tratar essa boolean gelo como tratas as 
direçoes (ir buscar no Awake e quando escolhes a sala ver se o filho também vai ter gelo).

De resto falta meter ifs e guardas em vários sitios em que o spawn ou assim pode não dar e algo dá null, e mini optimizações que estão aí nos TODO para
reduzir o uso de GetComponent
