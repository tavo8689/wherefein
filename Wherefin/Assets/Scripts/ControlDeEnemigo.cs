using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ControlDeEnemigo : MonoBehaviour
{
    private static readonly int EstaCaminando = Animator.StringToHash(name: "EstaCaminando");

    [Header("Referencias")]
    [SerializeField] private Transform[] puntosDePatrulla;


    [Header("Configuracion")]
    [SerializeField] private float tiempoDeEsperaDePatrulla = 2.0f;
    [SerializeField] private float distanciaDeParada = 0.5f;


    private NavMeshAgent _agent;
    private Animator _animator;
    private int _currentPatrolIndex;
    private bool _isWaiting;

    private void Awake()
    {
        _agent=GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        IrAlSiguientePuntoDePatrulla();
    }

    private void Update()
    {
        Patrulla();
        ActualizarAnimacion();
    }


    private void Patrulla()
    {
        if (_isWaiting) return;
        if (!_agent.pathPending && _agent.remainingDistance <= distanciaDeParada)
        {
            StartCoroutine(EsperarPuntoPatrulla());
        }
    }


    private IEnumerator EsperarPuntoPatrulla()
    {
        _isWaiting = true;
        _agent.isStopped = true;
        yield return new WaitForSeconds(tiempoDeEsperaDePatrulla);

        _agent.isStopped = false;
        IrAlSiguientePuntoDePatrulla();
        _isWaiting = false;
    }

    private void IrAlSiguientePuntoDePatrulla()
    {
        if (puntosDePatrulla.Length == 0) return;

        _agent.SetDestination(puntosDePatrulla[_currentPatrolIndex].position);
        _currentPatrolIndex = (_currentPatrolIndex + 1) % puntosDePatrulla.Length;
    }

    private void ActualizarAnimacion()
    {
        var estaCaminando = _agent.velocity.sqrMagnitude > 0.01f;
        _animator.SetBool(EstaCaminando, estaCaminando);
    }
}
