'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { CheckCircle, Dumbbell, Loader2 } from 'lucide-react';
import { useSearchParams } from 'next/navigation';
import axios from 'axios';
import { Suspense } from 'react';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api';

function ApplicationForm() {
  const searchParams = useSearchParams();
  const coachId = searchParams.get('coach') || '';

  const [submitted, setSubmitted] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  // Form state
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [age, setAge] = useState('');
  const [gender, setGender] = useState('');
  const [country, setCountry] = useState('');
  const [trainingExperience, setTrainingExperience] = useState('');
  const [currentSquat, setCurrentSquat] = useState('');
  const [currentBench, setCurrentBench] = useState('');
  const [currentDeadlift, setCurrentDeadlift] = useState('');
  const [goals, setGoals] = useState('');
  const [motivation, setMotivation] = useState('');
  const [message, setMessage] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!firstName || !lastName || !email || !coachId) return;

    setSubmitting(true);
    setError('');

    try {
      await axios.post(`${API_BASE_URL}/applications`, {
        coachId,
        firstName,
        lastName,
        email,
        phone: phone || undefined,
        age: age ? parseInt(age) : undefined,
        gender: gender || undefined,
        country: country || undefined,
        trainingExperience: trainingExperience || undefined,
        currentSquat: currentSquat ? parseFloat(currentSquat) : undefined,
        currentBench: currentBench ? parseFloat(currentBench) : undefined,
        currentDeadlift: currentDeadlift ? parseFloat(currentDeadlift) : undefined,
        goals: goals || undefined,
        motivation: motivation || undefined,
        message: message || undefined,
      });
      setSubmitted(true);
    } catch {
      setError('There was a problem submitting your application. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  if (!coachId) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4">
        <Card className="w-full max-w-md">
          <CardContent className="pt-6 text-center">
            <p className="text-slate-600">Invalid application link. Please contact your coach for the correct URL.</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (submitted) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4">
        <Card className="w-full max-w-md">
          <CardContent className="flex flex-col items-center py-12">
            <CheckCircle className="h-16 w-16 text-green-500 mb-4" />
            <h2 className="text-2xl font-bold text-slate-900 mb-2">Application Submitted!</h2>
            <p className="text-slate-600 text-center">
              Thank you for your application. Your coach will review it and get back to you soon.
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 py-8 px-4">
      <div className="max-w-2xl mx-auto">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-100 rounded-full mb-4">
            <Dumbbell className="h-8 w-8 text-blue-600" />
          </div>
          <h1 className="text-3xl font-bold text-slate-900">Coaching Application</h1>
          <p className="text-slate-600 mt-2">Fill out the form below to apply for coaching</p>
        </div>

        <form onSubmit={handleSubmit}>
          {/* Personal Information */}
          <Card className="mb-6">
            <CardHeader>
              <CardTitle className="text-lg">Personal Information</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="firstName">First Name *</Label>
                  <Input
                    id="firstName"
                    required
                    placeholder="John"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="lastName">Last Name *</Label>
                  <Input
                    id="lastName"
                    required
                    placeholder="Doe"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="email">Email *</Label>
                <Input
                  id="email"
                  type="email"
                  required
                  placeholder="john@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="phone">Phone</Label>
                  <Input
                    id="phone"
                    type="tel"
                    placeholder="+1 555 0123"
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="age">Age</Label>
                  <Input
                    id="age"
                    type="number"
                    min="13"
                    max="99"
                    placeholder="25"
                    value={age}
                    onChange={(e) => setAge(e.target.value)}
                  />
                </div>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Gender</Label>
                  <Select value={gender} onValueChange={setGender}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select..." />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Male">Male</SelectItem>
                      <SelectItem value="Female">Female</SelectItem>
                      <SelectItem value="Other">Other</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="country">Country</Label>
                  <Input
                    id="country"
                    placeholder="United States"
                    value={country}
                    onChange={(e) => setCountry(e.target.value)}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Training Background */}
          <Card className="mb-6">
            <CardHeader>
              <CardTitle className="text-lg">Training Background</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label>Experience Level</Label>
                <Select value={trainingExperience} onValueChange={setTrainingExperience}>
                  <SelectTrigger>
                    <SelectValue placeholder="How long have you been training?" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Beginner">Beginner (0-1 years)</SelectItem>
                    <SelectItem value="Intermediate">Intermediate (1-3 years)</SelectItem>
                    <SelectItem value="Advanced">Advanced (3-5 years)</SelectItem>
                    <SelectItem value="Elite">Elite (5+ years)</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <p className="text-sm text-slate-500">Current maxes (optional, in kg):</p>
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="squat">Squat</Label>
                  <Input
                    id="squat"
                    type="number"
                    min="0"
                    placeholder="0"
                    value={currentSquat}
                    onChange={(e) => setCurrentSquat(e.target.value)}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="bench">Bench Press</Label>
                  <Input
                    id="bench"
                    type="number"
                    min="0"
                    placeholder="0"
                    value={currentBench}
                    onChange={(e) => setCurrentBench(e.target.value)}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="deadlift">Deadlift</Label>
                  <Input
                    id="deadlift"
                    type="number"
                    min="0"
                    placeholder="0"
                    value={currentDeadlift}
                    onChange={(e) => setCurrentDeadlift(e.target.value)}
                  />
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Goals & Motivation */}
          <Card className="mb-6">
            <CardHeader>
              <CardTitle className="text-lg">Goals & Motivation</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="goals">What are your training goals?</Label>
                <Textarea
                  id="goals"
                  placeholder="e.g. Compete in powerlifting, improve general strength, etc."
                  value={goals}
                  onChange={(e) => setGoals(e.target.value)}
                  rows={3}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="motivation">Why do you want coaching?</Label>
                <Textarea
                  id="motivation"
                  placeholder="What made you decide to seek professional coaching?"
                  value={motivation}
                  onChange={(e) => setMotivation(e.target.value)}
                  rows={3}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="message">Anything else you&apos;d like to share?</Label>
                <Textarea
                  id="message"
                  placeholder="Additional information, injuries, schedule constraints, etc."
                  value={message}
                  onChange={(e) => setMessage(e.target.value)}
                  rows={3}
                />
              </div>
            </CardContent>
          </Card>

          {error && (
            <div className="mb-4 p-4 bg-red-50 text-red-700 rounded-lg text-sm">{error}</div>
          )}

          <Button type="submit" className="w-full h-12 text-base" disabled={submitting || !firstName || !lastName || !email}>
            {submitting ? (
              <>
                <Loader2 className="mr-2 h-5 w-5 animate-spin" />
                Submitting...
              </>
            ) : (
              'Submit Application'
            )}
          </Button>
        </form>
      </div>
    </div>
  );
}

export default function ApplyPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen bg-slate-50 flex items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
      </div>
    }>
      <ApplicationForm />
    </Suspense>
  );
}
