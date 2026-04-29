'use client';

import { useState } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Skeleton } from '@/components/ui/skeleton';
import { AthleteLiftHistory } from '@/components/athletes/lift-history';
import {
  ArrowLeft,
  User,
  Mail,
  Phone,
  MapPin,
  Calendar,
  Dumbbell,
  ClipboardPlus,
  CheckCircle,
} from 'lucide-react';
import { useAthlete, usePrograms, useAssignProgram, useAthleteProgram } from '@/hooks';
import { useCoach, useAuth } from '@/providers';
import { AthleteStatus } from '@/types';
import { format } from 'date-fns';
import { toast } from 'sonner';

function statusColor(status: AthleteStatus) {
  switch (status) {
    case 'Active': return 'bg-green-100 text-green-800';
    case 'Inactive': return 'bg-slate-100 text-slate-600';
    case 'OnHold': return 'bg-yellow-100 text-yellow-800';
    case 'Graduated': return 'bg-blue-100 text-blue-800';
    case 'Suspended': return 'bg-red-100 text-red-800';
    default: return 'bg-slate-100 text-slate-800';
  }
}

export default function AthleteDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const { user } = useAuth();
  const { coach } = useCoach();
  const coachId = user?.coachId ?? coach?.id ?? '';

  const { data: athlete, isLoading } = useAthlete(id);
  const { data: assignedProgram, isLoading: isProgramLoading } = useAthleteProgram(id);
  const { data: programsData } = usePrograms({
    coachId,
    isActive: true,
    pageSize: 100,
  });
  const assignProgram = useAssignProgram();

  // Assign Program dialog state
  const [assignOpen, setAssignOpen] = useState(false);
  const [selectedProgramId, setSelectedProgramId] = useState('');
  const [startDate, setStartDate] = useState(() => format(new Date(), 'yyyy-MM-dd'));

  const handleAssign = async () => {
    if (!selectedProgramId || !startDate || !coachId) return;
    try {
      await assignProgram.mutateAsync({
        athleteId: id,
        data: {
          programTemplateId: selectedProgramId,
          startDate,
        },
        coachId,
      });
      toast.success('Program assigned successfully');
      setAssignOpen(false);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { status?: number } };
      if (axiosErr.response?.status === 409) {
        toast.error('This athlete already has an active or pending program. Cancel it first before assigning a new one.');
      } else {
        toast.error('Failed to assign program. Please try again.');
      }
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-[400px] w-full" />
      </div>
    );
  }

  if (!athlete) {
    return (
      <div className="text-center py-12">
        <p className="text-slate-500">Athlete not found.</p>
        <Button variant="link" onClick={() => router.push('/athletes')}>
          Back to athletes
        </Button>
      </div>
    );
  }

  const programs = programsData?.items || [];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="sm" onClick={() => router.push('/athletes')}>
            <ArrowLeft className="h-4 w-4 mr-1" /> Back
          </Button>
          <div>
            <h1 className="text-2xl font-bold text-slate-900">{athlete.fullName}</h1>
            <p className="text-sm text-slate-500">
              {athlete.startDate
                ? `Since ${format(new Date(athlete.startDate), 'MMMM d, yyyy')}`
                : 'Recently added'}
            </p>
          </div>
          <Badge className={statusColor(athlete.status)}>{athlete.status}</Badge>
        </div>

        {!assignedProgram && (
          <Button onClick={() => setAssignOpen(true)}>
            <ClipboardPlus className="h-4 w-4 mr-2" />
            Assign Program
          </Button>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Personal Info */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Personal Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <InfoRow icon={<User className="h-4 w-4" />} label="Name" value={athlete.fullName} />
            <InfoRow icon={<Mail className="h-4 w-4" />} label="Email" value={athlete.email} />
            {athlete.phone && <InfoRow icon={<Phone className="h-4 w-4" />} label="Phone" value={athlete.phone} />}
            {athlete.country && <InfoRow icon={<MapPin className="h-4 w-4" />} label="Country" value={athlete.country} />}
            {athlete.dateOfBirth && (
              <InfoRow icon={<Calendar className="h-4 w-4" />} label="Date of Birth" value={format(new Date(athlete.dateOfBirth), 'MMM d, yyyy')} />
            )}
            {athlete.gender && <InfoRow label="Gender" value={athlete.gender} />}
          </CardContent>
        </Card>

        {/* Physical Details */}
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {athlete.height && <InfoRow label="Height" value={`${athlete.height} cm`} />}
            {athlete.weight && (
              <InfoRow icon={<Dumbbell className="h-4 w-4" />} label="Weight" value={`${athlete.weight} kg`} />
            )}
            {athlete.experienceLevel && (
              <InfoRow label="Experience" value={athlete.experienceLevel} />
            )}
            {athlete.goals && <InfoRow label="Goals" value={athlete.goals} />}
            {athlete.notes && <InfoRow label="Notes" value={athlete.notes} />}
          </CardContent>
        </Card>

        {/* Current Program */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="text-base">Current Program</CardTitle>
          </CardHeader>
          <CardContent>
            {isProgramLoading ? (
              <Skeleton className="h-20 w-full" />
            ) : assignedProgram ? (
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-medium text-slate-900">{assignedProgram.programName}</p>
                    <p className="text-sm text-slate-500">
                      {assignedProgram.startDate
                        ? `Started ${format(new Date(assignedProgram.startDate), 'MMM d, yyyy')}`
                        : 'Not started yet'}
                    </p>
                  </div>
                  <Badge variant="outline">
                    {assignedProgram.status}
                  </Badge>
                </div>
                {assignedProgram.workouts && assignedProgram.workouts.length > 0 && (
                  <div>
                    <p className="text-sm text-slate-500 mb-2">
                      {assignedProgram.workouts.filter((w: { isCompleted?: boolean }) => w.isCompleted).length} / {assignedProgram.workouts.length} workouts completed
                    </p>
                    <div className="w-full bg-slate-100 rounded-full h-2">
                      <div
                        className="bg-blue-600 h-2 rounded-full transition-all"
                        style={{
                          width: `${(assignedProgram.workouts.filter((w: { isCompleted?: boolean }) => w.isCompleted).length / assignedProgram.workouts.length) * 100}%`,
                        }}
                      />
                    </div>
                  </div>
                )}
              </div>
            ) : (
              <div className="text-center py-8">
                <ClipboardPlus className="h-8 w-8 mx-auto text-slate-300 mb-2" />
                <p className="text-slate-500 text-sm">No program assigned yet.</p>
                <Button variant="link" className="mt-1" onClick={() => setAssignOpen(true)}>
                  Assign a program
                </Button>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Lift History & PRs component */}
      {athlete && <AthleteLiftHistory athleteId={athlete.id} />}

      {/* Assign Program Dialog */}
      <Dialog open={assignOpen} onOpenChange={setAssignOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Assign Program to {athlete.fullName}</DialogTitle>
          </DialogHeader>
          <p className="text-sm text-slate-600">
            Selecting a program template will automatically generate all workout sessions for this athlete.
          </p>
          <div className="grid gap-4 py-2">
            <div className="grid gap-2">
              <Label>Program Template *</Label>
              <Select value={selectedProgramId} onValueChange={setSelectedProgramId}>
                <SelectTrigger>
                  <SelectValue placeholder="Select a program..." />
                </SelectTrigger>
                <SelectContent>
                  {programs.map((p) => (
                    <SelectItem key={p.id} value={p.id}>
                      {p.name} ({p.durationWeeks} weeks)
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid gap-2">
              <Label htmlFor="startDate">Start Date *</Label>
              <Input
                id="startDate"
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setAssignOpen(false)}>Cancel</Button>
            <Button
              onClick={handleAssign}
              disabled={!selectedProgramId || !startDate || assignProgram.isPending}
            >
              {assignProgram.isPending ? 'Assigning...' : (
                <>
                  <CheckCircle className="h-4 w-4 mr-2" />
                  Assign & Generate Workouts
                </>
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function InfoRow({
  icon,
  label,
  value,
}: {
  icon?: React.ReactNode;
  label: string;
  value: string;
}) {
  return (
    <div className="flex items-start gap-3">
      {icon && <span className="mt-0.5 text-slate-400">{icon}</span>}
      <div>
        <p className="text-sm font-medium text-slate-500">{label}</p>
        <p className="text-sm text-slate-900">{value}</p>
      </div>
    </div>
  );
}
