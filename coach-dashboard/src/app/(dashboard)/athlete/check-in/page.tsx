'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/providers';
import { useCreateCheckIn, useAthleteCheckIns } from '@/hooks';
import { routes } from '@/config/routes';
import { toast } from 'sonner';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  ArrowLeft,
  Send,
  Scale,
  Battery,
  Moon,
  Brain,
  Pizza,
  Dumbbell,
  CheckCircle2,
  Clock,
  ClipboardCheck,
  MessageSquare,
} from 'lucide-react';
import { format } from 'date-fns';

// Slider-like rating component
function RatingSelector({
  label,
  icon,
  value,
  onChange,
  max = 10,
  lowLabel = 'Low',
  highLabel = 'High',
}: {
  label: string;
  icon: React.ReactNode;
  value: number | null;
  onChange: (v: number | null) => void;
  max?: number;
  lowLabel?: string;
  highLabel?: string;
}) {
  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <Label className="flex items-center gap-2 text-sm font-medium">
          {icon}
          {label}
        </Label>
        {value !== null && (
          <span className="text-sm font-bold text-blue-600">{value}/{max}</span>
        )}
      </div>
      <div className="flex gap-1">
        {Array.from({ length: max }, (_, i) => i + 1).map((n) => (
          <button
            key={n}
            type="button"
            onClick={() => onChange(value === n ? null : n)}
            className={`flex-1 h-9 rounded-md text-xs font-semibold transition-all ${
              value !== null && n <= value
                ? n <= 3
                  ? 'bg-red-500 text-white'
                  : n <= 6
                  ? 'bg-amber-500 text-white'
                  : 'bg-green-500 text-white'
                : 'bg-slate-100 text-slate-400 hover:bg-slate-200'
            }`}
          >
            {n}
          </button>
        ))}
      </div>
      <div className="flex justify-between text-xs text-slate-400">
        <span>{lowLabel}</span>
        <span>{highLabel}</span>
      </div>
    </div>
  );
}

export default function AthleteCheckInPage() {
  const { user } = useAuth();
  const router = useRouter();
  const athleteId = user?.athleteId || '';

  const createCheckIn = useCreateCheckIn();
  const { data: recentCheckIns, isLoading: loadingHistory } = useAthleteCheckIns(athleteId, 1, 5);

  // Form state
  const [weight, setWeight] = useState('');
  const [energyLevel, setEnergyLevel] = useState<number | null>(null);
  const [sleepQuality, setSleepQuality] = useState<number | null>(null);
  const [sleepHours, setSleepHours] = useState('');
  const [stressLevel, setStressLevel] = useState<number | null>(null);
  const [nutritionAdherence, setNutritionAdherence] = useState<number | null>(null);
  const [trainingAdherence, setTrainingAdherence] = useState<number | null>(null);
  const [notes, setNotes] = useState('');

  const handleSubmit = async () => {
    try {
      await createCheckIn.mutateAsync({
        athleteId,
        weight: weight ? parseFloat(weight) : null,
        weightUnit: 'kg',
        energyLevel,
        sleepQuality,
        sleepHours: sleepHours ? parseFloat(sleepHours) : null,
        stressLevel,
        nutritionAdherence,
        trainingAdherence,
        notes: notes.trim() || undefined,
      });
      toast.success('Check-in submitted!');
      // Reset form
      setWeight('');
      setEnergyLevel(null);
      setSleepQuality(null);
      setSleepHours('');
      setStressLevel(null);
      setNutritionAdherence(null);
      setTrainingAdherence(null);
      setNotes('');
    } catch {
      toast.error('Failed to submit check-in');
    }
  };

  return (
    <div className="space-y-6 max-w-2xl mx-auto">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={() => router.push(routes.athleteDashboard)}>
          <ArrowLeft className="h-5 w-5" />
        </Button>
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Weekly Check-in</h1>
          <p className="text-sm text-slate-500">{format(new Date(), 'EEEE, MMMM d, yyyy')}</p>
        </div>
      </div>

      {/* Check-in Form */}
      <Card className="shadow-sm">
        <CardHeader className="pb-4 border-b">
          <CardTitle className="text-lg">How are you feeling?</CardTitle>
          <p className="text-sm text-slate-500">Rate your wellness and log your progress. All fields are optional.</p>
        </CardHeader>
        <CardContent className="p-6 space-y-8">
          {/* Body Weight */}
          <div className="space-y-2">
            <Label className="flex items-center gap-2 text-sm font-medium">
              <Scale className="h-4 w-4 text-slate-500" />
              Body Weight
            </Label>
            <div className="flex items-center gap-2">
              <Input
                type="number"
                step="0.1"
                placeholder="e.g. 80.5"
                value={weight}
                onChange={(e) => setWeight(e.target.value)}
                className="max-w-[160px]"
              />
              <span className="text-sm text-slate-400">kg</span>
            </div>
          </div>

          {/* Sleep Hours */}
          <div className="space-y-2">
            <Label className="flex items-center gap-2 text-sm font-medium">
              <Moon className="h-4 w-4 text-indigo-500" />
              Hours of Sleep
            </Label>
            <div className="flex items-center gap-2">
              <Input
                type="number"
                step="0.5"
                min="0"
                max="24"
                placeholder="e.g. 7.5"
                value={sleepHours}
                onChange={(e) => setSleepHours(e.target.value)}
                className="max-w-[160px]"
              />
              <span className="text-sm text-slate-400">hours</span>
            </div>
          </div>

          {/* Rating Fields */}
          <div className="space-y-6">
            <RatingSelector
              label="Energy Level"
              icon={<Battery className="h-4 w-4 text-yellow-500" />}
              value={energyLevel}
              onChange={setEnergyLevel}
              lowLabel="Exhausted"
              highLabel="Energized"
            />

            <RatingSelector
              label="Sleep Quality"
              icon={<Moon className="h-4 w-4 text-indigo-500" />}
              value={sleepQuality}
              onChange={setSleepQuality}
              lowLabel="Poor"
              highLabel="Excellent"
            />

            <RatingSelector
              label="Stress Level"
              icon={<Brain className="h-4 w-4 text-red-500" />}
              value={stressLevel}
              onChange={setStressLevel}
              lowLabel="Very stressed"
              highLabel="Relaxed"
            />

            <RatingSelector
              label="Nutrition Adherence"
              icon={<Pizza className="h-4 w-4 text-orange-500" />}
              value={nutritionAdherence}
              onChange={setNutritionAdherence}
              lowLabel="Off plan"
              highLabel="On point"
            />

            <RatingSelector
              label="Training Adherence"
              icon={<Dumbbell className="h-4 w-4 text-blue-500" />}
              value={trainingAdherence}
              onChange={setTrainingAdherence}
              lowLabel="Missed sessions"
              highLabel="All sessions"
            />
          </div>

          {/* Notes */}
          <div className="space-y-2">
            <Label className="flex items-center gap-2 text-sm font-medium">
              <MessageSquare className="h-4 w-4 text-slate-500" />
              Notes for your coach
            </Label>
            <Textarea
              placeholder="Any injuries, motivation, questions, or other observations..."
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={4}
            />
          </div>

          {/* Submit */}
          <Button
            onClick={handleSubmit}
            disabled={createCheckIn.isPending}
            className="w-full"
            size="lg"
          >
            {createCheckIn.isPending ? (
              <>Submitting...</>
            ) : (
              <>
                <Send className="h-4 w-4 mr-2" />
                Submit Check-in
              </>
            )}
          </Button>
        </CardContent>
      </Card>

      {/* Recent Check-ins */}
      <Card className="shadow-sm">
        <CardHeader className="pb-3">
          <CardTitle className="text-lg flex items-center gap-2">
            <Clock className="h-5 w-5 text-slate-500" />
            Recent Check-ins
          </CardTitle>
        </CardHeader>
        <CardContent>
          {loadingHistory ? (
            <div className="space-y-3">
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} className="h-14 w-full rounded-xl" />
              ))}
            </div>
          ) : recentCheckIns && recentCheckIns.items.length > 0 ? (
            <div className="space-y-3">
              {recentCheckIns.items.map((ci) => (
                <div
                  key={ci.id}
                  className="flex items-center justify-between p-4 rounded-xl border border-slate-100 bg-slate-50/50"
                >
                  <div className="flex items-center gap-3">
                    <CheckCircle2 className="h-5 w-5 text-green-500 shrink-0" />
                    <div>
                      <p className="font-medium text-slate-800 text-sm">
                        {format(new Date(ci.checkInDate || ci.createdAt), 'MMM d, yyyy')}
                      </p>
                      <div className="flex items-center gap-2 mt-0.5">
                        {ci.weight && (
                          <span className="text-xs text-slate-500">{ci.weight} kg</span>
                        )}
                        {ci.energyLevel && (
                          <span className="text-xs text-slate-500">Energy: {ci.energyLevel}/10</span>
                        )}
                      </div>
                    </div>
                  </div>
                  {ci.coachFeedback ? (
                    <Badge variant="secondary" className="bg-green-100 text-green-700 text-xs">
                      Reviewed
                    </Badge>
                  ) : (
                    <Badge variant="secondary" className="bg-slate-100 text-slate-500 text-xs">
                      Pending
                    </Badge>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8">
              <ClipboardCheck className="h-10 w-10 mx-auto text-slate-300 mb-3" />
              <p className="text-slate-500 text-sm">No check-ins submitted yet</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
