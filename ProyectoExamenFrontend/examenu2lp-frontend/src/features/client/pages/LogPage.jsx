import React, { useState } from "react";

export function LogPage() {
  const [logs, setLogs] = useState([
    { action: "Crear Partida", date: "2024-11-15", userId: "U001" },
    { action: "Editar Partida", date: "2024-11-14", userId: "U002" },
    { action: "Eliminar Partida", date: "2024-11-13", userId: "U003" },
    { action: "Consultar Log", date: "2024-11-12", userId: "U001" },
  ]);

  return (
    <div className="w-full max-w-6xl mx-auto p-4">
      <div className="bg-white shadow-lg rounded-lg">
        <div className="p-4 border-b">
          <h1 className="text-2xl font-bold">Registro de Actividades</h1>
        </div>
        {/* Tabla de registros */}
        <div className="p-4">
          <table className="w-full border-collapse">
            <thead>
              <tr>
                <th className="text-left text-sm font-semibold text-gray-700 border-b pb-2">Acción</th>
                <th className="text-left text-sm font-semibold text-gray-700 border-b pb-2">Fecha</th>
                <th className="text-left text-sm font-semibold text-gray-700 border-b pb-2">ID de Usuario</th>
              </tr>
            </thead>
            <tbody>
              {logs.map((log, index) => (
                <tr key={index} className="border-t">
                  <td className="p-2">{log.action}</td>
                  <td className="p-2">{log.date}</td>
                  <td className="p-2">{log.userId}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
