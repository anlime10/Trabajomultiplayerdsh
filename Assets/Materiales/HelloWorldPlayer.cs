using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        
        [Header("Configuración Visual")] // Crea un título en el Inspector
        [Tooltip("Arrastra aquí los materiales que quieres que usen los jugadores")]
        public Material[] availableMaterials; 
        public NetworkVariable<int> materialIndex = new NetworkVariable<int>();
        private MeshRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        public override void OnNetworkSpawn()
        {
            // 2. Suscribirse al cambio para que se actualice en todos los clientes
            materialIndex.OnValueChanged += OnMaterialChanged;
            
            // Aplicar el material actual (por si entramos tarde a la partida)
            ApplyMaterial(materialIndex.Value);

            if (IsOwner)
            {
                Move();
            }
        }

         // 3. Solo el servidor decide qué material le toca al jugador
        public override void OnNetworkDespawn() 
        {
            materialIndex.OnValueChanged -= OnMaterialChanged;
        }

        private void OnMaterialChanged(int previousValue, int newValue)
        {
            ApplyMaterial(newValue);
        }

      private void ApplyMaterial(int index)
        {
            if (availableMaterials != null && index < availableMaterials.Length)
            {
                _renderer.material = availableMaterials[index];
            }
        }





        public void Move()
        {
            SubmitPositionRequestRpc();
        }

        [Rpc(SendTo.Server)]
        private void SubmitPositionRequestRpc(RpcParams rpcParams = default)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;

            // 4. El servidor asigna un índice aleatorio del array
            if (availableMaterials.Length > 0)
            {
                materialIndex.Value = Random.Range(0, availableMaterials.Length);
            }
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        private void Update()
        {
            if (!IsOwner)
            {
                return;
                
            }
            Vector3 position = new Vector3 (Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Time.deltaTime * 3f;
            transform.position += position;
        }
    }
}