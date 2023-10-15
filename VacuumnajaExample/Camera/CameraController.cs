using MoreMountains.Tools;
using UnityEngine;

namespace _Scripts.Camera
{
    public class CameraController : MMSingleton<CameraController>
    {
        public Transform _target; // Ссылка на объект планеты
        public float _distance = 10f; // Изначальное расстояние от камеры до планеты
        public float _zoomSpeed = 1f; // Скорость приближения/отдаления
        public float _rotationSpeed = 1f; // Скорость вращения
        public float _minDistance = 5f; // Минимальное расстояние приближения
        public float _maxDistance = 20f; // Максимальное расстояние отдаления

        public bool _invertVertical; // Инвертировать клавиши W и S
        public bool _invertHorizontal; // Инвертировать клавиши A и D

        private float _currentDistance; // Текущее расстояние от камеры до планеты

        private float _latitude; // Широта
        private float _longitude; // Долгота

        private void Start()
        {
            _currentDistance = _distance;
        }

        private void LateUpdate()
        {
            // Приближение/отдаление камеры с помощью колеса мыши
            float zoomAmount = Input.GetAxis("Mouse ScrollWheel") * _zoomSpeed;
            _currentDistance -= zoomAmount;
            _currentDistance = Mathf.Clamp(_currentDistance, _minDistance, _maxDistance);

            // Изменение широты с помощью клавиш W и S
            float verticalInput = _invertVertical ? -Input.GetAxis("Vertical") : Input.GetAxis("Vertical");
            _latitude += verticalInput;

            // Ограничение широты от -90 до 90 градусов
            _latitude = Mathf.Clamp(_latitude, -90f, 90f);

            // Изменение долготы с помощью клавиш A и D
            float horizontalInput = _invertHorizontal ? -Input.GetAxis("Horizontal") : Input.GetAxis("Horizontal");
            _longitude += horizontalInput * _rotationSpeed;

            // Вращение камеры вокруг планеты
            Quaternion rotation = Quaternion.Euler(_latitude, _longitude, 0f);
            Vector3 position = rotation * new Vector3(0f, 0f, -_currentDistance) + _target.position;

            transform.rotation = rotation;
            transform.position = position;
        }

        public void SetActiveState(bool state)
        {
            enabled = state;
        }
    }
}