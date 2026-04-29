'use client';

import React, { useState } from 'react';
import { HelpCircle } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Badge } from '@/components/ui/badge';

interface NotationExample {
  notation: string;
  description: string;
  meaning: string;
}

interface NotationCategory {
  title: string;
  description: string;
  examples: NotationExample[];
}

const NOTATION_CATEGORIES: NotationCategory[] = [
  {
    title: 'Notaciones Básicas',
    description: 'Las formas más simples y comunes de representar series',
    examples: [
      {
        notation: '3x8',
        description: '3 series de 8 reps',
        meaning: 'Realiza 3 veces el ejercicio con 8 repeticiones cada una',
      },
      {
        notation: '4x8-12',
        description: '4 series de 8 a 12 reps (rango)',
        meaning: 'Realiza 4 series con reps entre 8 y 12, según sensación',
      },
      {
        notation: '5x5',
        description: '5 series de 5 reps',
        meaning: 'Forma clásica de entrenamiento de fuerza',
      },
    ],
  },
  {
    title: 'Con RPE (Rating of Perceived Exertion)',
    description: 'Especifica la intensidad percibida del 1 (muy fácil) al 10 (máximo esfuerzo)',
    examples: [
      {
        notation: '3x10 @8',
        description: '3 series de 10 reps a RPE 8',
        meaning: 'Realiza con una intensidad que dejaría 2 reps "en el tanque"',
      },
      {
        notation: '4x6 @9',
        description: '4 series de 6 reps a RPE 9',
        meaning: 'Muy intenso, solo 1 rep dejaría en el tanque',
      },
      {
        notation: '5x5-8 @8',
        description: 'Rango con RPE especificado',
        meaning: 'Reps entre 5-8 manteniendo RPE 8',
      },
    ],
  },
  {
    title: 'Con Porcentaje del 1RM',
    description: 'Basado en el 1 rep max (máxima carga que puede levantar)',
    examples: [
      {
        notation: '3x8 65%',
        description: '3 series de 8 reps al 65% del 1RM',
        meaning: 'El peso es el 65% de tu máximo',
      },
      {
        notation: '1x1 3x4 84% 74%',
        description: 'Primer grupo al 84%, segundo al 74%',
        meaning: 'Progresión con porcentajes específicos por grupo',
      },
      {
        notation: '5x3 @80%',
        description: 'Also valid: al 80% del 1RM',
        meaning: 'Notación alternativa con símbolo @',
      },
    ],
  },
  {
    title: 'Con Peso Directo',
    description: 'Especifica el peso exacto a usar',
    examples: [
      {
        notation: '3x5 @225',
        description: '3 series de 5 reps con 225kg/lb',
        meaning: 'Peso específico, se usa el indicado sin importar 1RM',
      },
      {
        notation: '3x5 @225kg',
        description: 'Con unidad de peso',
        meaning: 'Clarifica que es kilogramos',
      },
      {
        notation: '5x3 @185lb',
        description: 'Con unidad en libras',
        meaning: 'Para coaches que trabajan en imperial',
      },
    ],
  },
  {
    title: 'Notaciones Compuestas/Backoff Sets',
    description: 'Múltiples grupos de series en un mismo ejercicio',
    examples: [
      {
        notation: '1x1 3x4',
        description: '1 single + 3 backoff sets de 4 reps',
        meaning: 'Sube a un peso máximo (1 rep), luego baja 10-15% y haz 3x4',
      },
      {
        notation: '1x1 3x4 84% 74%',
        description: 'Con porcentajes específicos',
        meaning: 'Single al 84%, luego 3x4 al 74% del 1RM',
      },
      {
        notation: '2x2 3x5 3x8',
        description: 'Tres grupos diferentes',
        meaning: 'Pirámide inversa descendente de intensidad',
      },
    ],
  },
  {
    title: 'Notaciones Especiales',
    description: 'Formatos únicos para tipos específicos de entrenamientos',
    examples: [
      {
        notation: 'Single @9',
        description: 'Una sola repetición a RPE 9',
        meaning: 'Trabajo de fuerza máxima, muy intenso',
      },
      {
        notation: 'AMRAP',
        description: 'As Many Reps As Possible (tantas reps como sea posible)',
        meaning: 'HacerMaxReps en el tiempo/forma permitido, genera volumen',
      },
      {
        notation: '3x10 EMOM 6',
        description: 'Every Minute On the Minute por 6 minutos',
        meaning: 'Haz el set cada minuto exacto, descansa el tiempo restante',
      },
    ],
  },
  {
    title: 'Con Tempo',
    description: 'Especifica la velocidad de movimiento: Excéntrico:Pausa:Concéntrico',
    examples: [
      {
        notation: '3x8 TEMPO 3:1:0',
        description: '3:1:0 = 3 seg bajada, 1 seg pausa, 0 seg subida rápida',
        meaning: 'Control excéntrico importante, pausa en el fondo',
      },
      {
        notation: '3x10 TEMPO 2:0:1',
        description: 'Control concéntrico importante',
        meaning: 'Subida lenta y controlada, bajada rápida',
      },
      {
        notation: '4x6 @8 TEMPO 4:2:1',
        description: 'Puede combinarse con RPE',
        meaning: 'Tempo muy controlado con intensidad moderada',
      },
    ],
  },
];

interface NotationHelpModalProps {
  /** Show as icon button or text button */
  variant?: 'icon' | 'text';
  /** Button size */
  size?: 'sm' | 'md' | 'lg';
}

export function NotationHelpModal({ variant = 'icon', size = 'md' }: NotationHelpModalProps) {
  const [open, setOpen] = useState(false);

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        {variant === 'icon' ? (
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8"
            title="Ver ayuda de notaciones"
          >
            <HelpCircle className="h-4 w-4" />
          </Button>
        ) : (
          <Button variant="outline" size={size === 'sm' ? 'sm' : size === 'lg' ? 'lg' : 'default'}>
            <HelpCircle className="h-4 w-4 mr-2" />
            Ayuda de Notaciones
          </Button>
        )}
      </DialogTrigger>

      <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Notaciones de Series Soportadas</DialogTitle>
          <DialogDescription>
            Guía completa de todas las formato de notación que puedes usar en el builder
          </DialogDescription>
        </DialogHeader>

        <Tabs defaultValue="0" className="w-full">
          <TabsList className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 w-full">
            {NOTATION_CATEGORIES.map((category, idx) => (
              <TabsTrigger key={idx} value={idx.toString()} className="text-xs sm:text-sm">
                {category.title.split('/')[0]}
              </TabsTrigger>
            ))}
          </TabsList>

          {NOTATION_CATEGORIES.map((category, idx) => (
            <TabsContent key={idx} value={idx.toString()} className="space-y-4">
              <div>
                <h3 className="text-lg font-semibold mb-2">{category.title}</h3>
                <p className="text-sm text-muted-foreground mb-4">{category.description}</p>
              </div>

              <div className="space-y-3">
                {category.examples.map((example, exIdx) => (
                  <div
                    key={exIdx}
                    className="border border-border rounded-lg p-4 space-y-2 hover:bg-muted/30 transition-colors"
                  >
                    <div className="flex items-start justify-between gap-2">
                      <Badge variant="secondary" className="font-mono text-sm whitespace-nowrap">
                        {example.notation}
                      </Badge>
                    </div>
                    <div>
                      <p className="font-medium text-sm">{example.description}</p>
                      <p className="text-sm text-muted-foreground mt-1">{example.meaning}</p>
                    </div>
                  </div>
                ))}
              </div>
            </TabsContent>
          ))}
        </Tabs>

        <div className="mt-6 pt-4 border-t space-y-2">
          <h4 className="font-semibold text-sm">Consejos útiles:</h4>
          <ul className="text-sm text-muted-foreground space-y-1 list-disc list-inside">
            <li>
              <strong>Combina notaciones:</strong> Puedes mezclar RPE con Tempo: <code>3x8 @8 TEMPO 3:1:0</code>
            </li>
            <li>
              <strong>El builder es flexible:</strong> Intenta escribir lo que tengas en mente, probablemente lo parsee
            </li>
            <li>
              <strong>EMOM vs repeticiones:</strong> EMOM es útil para control del ritmo y densidad de entrenamiento
            </li>
            <li>
              <strong>Rango de reps:</strong> El rango (8-12) es flexible y depende del día y sensación del atleta
            </li>
          </ul>
        </div>
      </DialogContent>
    </Dialog>
  );
}

/**
 * Inline notation example component for tooltips
 */
export function NotationExample({ notation }: { notation: string }) {
  // Find the example in categories
  for (const category of NOTATION_CATEGORIES) {
    const found = category.examples.find((ex) => ex.notation === notation);
    if (found) {
      return (
        <div className="space-y-1">
          <div className="font-medium text-sm">{found.description}</div>
          <div className="text-xs text-muted-foreground">{found.meaning}</div>
        </div>
      );
    }
  }

  return null;
}
