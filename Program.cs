using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Lista de tareas para simular el procesamiento de varios pedidos
        List<Task> pedidos = new List<Task>();

        // Creación y lanzamiento de 5 pedidos
        for (int i = 1; i <= 5; i++)
        {
            int pedidoId = i;
            // Usamos Task.Run para iniciar el procesamiento de cada pedido
            var pedidoTask = Task.Run(() => ProcesarPedido(pedidoId));
            pedidos.Add(pedidoTask);
        }

        // Uso de Task.WhenAny para detectar cuál pedido se procesa primero
        Task primerPedidoCompleto = await Task.WhenAny(pedidos);
        Console.WriteLine("El primer pedido procesado fue el número: " + (pedidos.IndexOf(primerPedidoCompleto) + 1));

        // Esperar a que todos los pedidos finalicen
        await Task.WhenAll(pedidos);
        Console.WriteLine("Todos los pedidos han sido procesados.");
    }

    static Task ProcesarPedido(int pedidoId)
    {
        // Tarea padre que abarca el procesamiento completo del pedido
        return Task.Factory.StartNew(() =>
        {
            Console.WriteLine($"Iniciando procesamiento del pedido {pedidoId}");

            // Tareas hijo para cada etapa del procesamiento, adjuntas a la tarea padre
            var verificacionStock = Task.Factory.StartNew(async () => {
                await Task.Delay(500); // Simula verificación de stock
                Console.WriteLine($"Pedido {pedidoId}: Stock verificado.");
            }, TaskCreationOptions.AttachedToParent).Unwrap();

            var procesamientoPago = Task.Factory.StartNew(async () => {
                await Task.Delay(700); // Simula procesamiento del pago
                Console.WriteLine($"Pedido {pedidoId}: Pago procesado.");
            }, TaskCreationOptions.AttachedToParent).Unwrap();

            var preparacionEnvio = Task.Factory.StartNew(async () => {
                await Task.Delay(600); // Simula preparación del envío
                Console.WriteLine($"Pedido {pedidoId}: Envío preparado.");
            }, TaskCreationOptions.AttachedToParent).Unwrap();

            // Continuación que se ejecuta únicamente si todas las tareas hijo se completan exitosamente
            Task finalizacion = Task.WhenAll(verificacionStock, procesamientoPago, preparacionEnvio)
                .ContinueWith(t =>
                {
                    Console.WriteLine($"Pedido {pedidoId}: Procesamiento completado exitosamente.");
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            // Manejo de error: Si alguna tarea se cancela o falla, se ejecuta esta continuación
            finalizacion.ContinueWith(t =>
            {
                Console.WriteLine($"Pedido {pedidoId}: Error en el procesamiento.");
            }, TaskContinuationOptions.OnlyOnCanceled);

            // Esperamos la finalización de las tareas para mantener la sincronía en este ejemplo
            finalizacion.Wait();
        });
    }
}
